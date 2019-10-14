using System;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;


using Newtonsoft.Json;
using CommandLine;
using CommandLine.Text;

using Colorful;
using CO = Colorful.Console;
using Figgle;
using ConsoleTables;

using Canaan;

namespace NewsAlpha
{
    class Program : Api
    {
        #region Enums
        public enum ExitResult
        {
            SUCCESS = 0,
            UNHANDLED_EXCEPTION = 1,
            INVALID_OPTIONS = 2
        }
        #endregion

        #region Entrypoint
        static void Main(string[] args)
        {
            SetLogger(new SerilogLogger("NewsAlpha-CLI.log"));
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            CO.WriteLine(FiggleFonts.Chunky.Render("News Alpha"), Color.Blue);
            CO.WriteLine("v{0}", AssemblyVersion.ToString(3), Color.Blue);

            ParserResult<object> result = new Parser().ParseArguments<SocialNewsOptions, GabOptions, FourChanPolOptions, ArticleOptions>(args);
            result.WithNotParsed((IEnumerable<Error> errors) =>
            {
                HelpText help = GetAutoBuiltHelpText(result);

                help.Copyright = string.Empty;
                help.AddPreOptionsLine(string.Empty);

                if (errors.Any(e => e.Tag == ErrorType.VersionRequestedError))
                {
                    Exit(ExitResult.SUCCESS);
                }
                else if (errors.Any(e => e.Tag == ErrorType.HelpVerbRequestedError))
                {
                    HelpVerbRequestedError error = (HelpVerbRequestedError)errors.First(e => e.Tag == ErrorType.HelpVerbRequestedError);
                    if (error.Type != null)
                    {
                        help.AddVerbs(error.Type);
                    }
                    else
                    {
                        help.AddVerbs(typeof(GabOptions), typeof(FourChanPolOptions), typeof(ArticleOptions));
                    }
                    Info(help);
                    Exit(ExitResult.SUCCESS);
                }
                else if (errors.Any(e => e.Tag == ErrorType.HelpRequestedError))
                {
                    help.AddVerbs(typeof(GabOptions), typeof(FourChanPolOptions), typeof(ArticleOptions));
                    Info(help);
                    Exit(ExitResult.SUCCESS);
                }
                else if (errors.Any(e => e.Tag == ErrorType.NoVerbSelectedError))
                {
                    help.AddVerbs(typeof(GabOptions), typeof(FourChanPolOptions), typeof(ArticleOptions));
                    Error("No index selected. Specify one of: gab, pol.");
                    Info(help);
                    Exit(ExitResult.INVALID_OPTIONS);
                }
                else if (errors.Any(e => e.Tag == ErrorType.MissingRequiredOptionError))
                {
                    MissingRequiredOptionError error = (MissingRequiredOptionError)errors.First(e => e.Tag == ErrorType.MissingRequiredOptionError);
                    Error("A required option is missing: {0}.", error.NameInfo.NameText);
                    Info(help);
                    Exit(ExitResult.INVALID_OPTIONS);
                }
                else if (errors.Any(e => e.Tag == ErrorType.UnknownOptionError))
                {
                    UnknownOptionError error = (UnknownOptionError)errors.First(e => e.Tag == ErrorType.UnknownOptionError);
                    help.AddVerbs(typeof(GabOptions), typeof(FourChanPolOptions), typeof(ArticleOptions));
                    Error("Unknown option: {error}.", error.Token);
                    Info(help);
                    Exit(ExitResult.INVALID_OPTIONS);
                }
                else
                {
                    Error("An error occurred parsing the program options: {errors}.", errors);
                    help.AddVerbs(typeof(GabOptions), typeof(FourChanPolOptions), typeof(ArticleOptions));
                    Info(help);
                    Exit(ExitResult.INVALID_OPTIONS);
                }
            })
            .WithParsed<GabOptions>(o =>
            {
                Search("gab", o.Search, o.ThreatIntent, o.IdentityHate, o.Links, o.Count).Wait();
                Exit(ExitResult.SUCCESS);
            })
            .WithParsed<FourChanPolOptions>(o =>
            {
                Search("4chpol", o.Search, o.ThreatIntent, o.IdentityHate, o.Links, o.Count).Wait();
                Exit(ExitResult.SUCCESS);
            })
            .WithParsed<ArticleOptions>(o =>
            {
                if (!string.IsNullOrEmpty(o.AddTopic))
                {
                    AddNewsTopic(o.AddTopic, o.Count).Wait();
                    Exit(ExitResult.SUCCESS);
                }
                else if (!string.IsNullOrEmpty(o.RetrieveTopic))
                {
                    RetrieveNewsTopic(o.RetrieveTopic, o.Count).Wait();
                    Exit(ExitResult.SUCCESS);
                }
            });
        }
        #endregion

        #region Methods
        static async Task Search(string index, string query, double threatIntent, bool identityhate, bool search_links, int top)
        {
            var client = new SearchIndexClient("socialnews", index, new SearchCredentials("0FD303356E9F04596A433B00CCE37F1E"));
            client.SetUserAgent("NewsAlphaCLI");
            string field = "text", filterMsg = "none", limit = "all";
            SearchParameters parameters = new SearchParameters()
            {
                Select = new[] { "id", "user", "text", "date_published", "no", "links", "identity_hate", "entities", "threat_intent"},
                OrderBy = new [] {"date_published desc"}
            };
            if (threatIntent > 0.0)
            {
                parameters.Filter = "threat_intent gt " + threatIntent;
                parameters.OrderBy = new[] { "threat_intent desc, date_published desc" };
                filterMsg = "threat_intent";
            }
         
            if (identityhate && threatIntent > 0.0)
            {
                parameters.Filter = "and identity_hate eq true";
                filterMsg = "threat_intent and identity_hate";
                
            }
            else if (identityhate)
            {
                parameters.Filter = "identity_hate eq true";
                filterMsg = "identity_hate";
            }

            if (search_links)
            {
                parameters.SearchFields = new[] { "links/uri" };
                field = "links";
            }
            if (top > 0)
            {
                parameters.Top = top;
                limit = top.ToString();
            }
            DocumentSearchResult<Document> docResults = null;
            List<SocialNewsSearchResult> results = new List<SocialNewsSearchResult>();
            
            using (var op = Begin("Search {0} for {1} containing {2} filtered on {3} limited to {4} results", 
                index, field, query, filterMsg, limit))
            {
                docResults = await client.Documents.SearchAsync(query, parameters);
                op.Complete();
            }
            Info("Got {0} results.", docResults.Results.Count);
            foreach(var dr in docResults.Results)
            {
                var r = new SocialNewsSearchResult()
                {
                    Id = (string)dr.Document["id"],
                    User = (string)dr.Document["user"],
                    DatePublished = ((DateTimeOffset) dr.Document["date_published"]).UtcDateTime,
                    Text = (string) dr.Document["text"],
                    HasIdentityHate = dr.Document["identity_hate"] != null ? (bool) dr.Document["identity_hate"] : false,
                    No = (long) dr.Document["no"],
                    ThreatIntent = dr.Document["threat_intent"] != null ? (double) dr.Document["threat_intent"] : 0.0,
                };
                try
                {
                    var links = (IEnumerable<Document>)dr.Document["links"];
                    r.Links = links.Select(l => (string)l["uri"]).ToArray();

                    var entities = (object[]) dr.Document["entities"];
                    r.Entities = entities.Cast<string>().ToArray();

                }
                catch
                {
                    r.Links = Array.Empty<string>();
                }
                results.Add(r);
            }
            if (results.Count == 0)
            {
                Info("0 social news posts in index {0} matching query {1}.\n", index.Replace("-index", ""), query);
                Exit(ExitResult.SUCCESS);
            }
            Info("Showing social news posts in index {0} matching query {1}.\n", index.Replace("-index", ""), query); 
            CO.WriteLine("----");
            foreach (var r in results)
            {
                CO.WriteLineFormatted("Id: {0}\nDate: {1} (UTC) \nUser: {2}\nNo.: {3}\nText: {4}\nLinks: \n{5}\nEntities: \n{6}", 
                    Color.LightGoldenrodYellow, Color.Gray, r.Id, r.DatePublished.ToString(), r.User, r.No, r.Text, "\t" + string.Join(Environment.NewLine + "\t", r.Links), 
                        r.Entities == null ? "" : "\t" + string.Join(Environment.NewLine + "\t", r.Entities));
                if (r.ThreatIntent > 0.91)
                {
                    CO.WriteLineFormatted("Threat Intent: {0}", Color.Red, Color.Gray, r.ThreatIntent);
                }
                else
                {
                    CO.WriteLineFormatted("Threat Intent: {0}", Color.LightGoldenrodYellow, Color.Gray, r.ThreatIntent);
                }
                CO.WriteLine("----");
            }  
        }

        static async Task AddNewsTopic(string topic, int articleCount)
        {
            BingNewsAzure pipeline = new BingNewsAzure(topic);
            await pipeline.InsertArticlesForTopic(topic, 2019, articleCount);
        }

        static async Task RetrieveNewsTopic(string topic, int articleCount)
        {
            BingNewsAzure pipeline = new BingNewsAzure(topic);
            await pipeline.AnalyzeArticleImages(new List<Article>());
        }

        static void Exit(ExitResult result)
        {
        
            if (Cts != null)
            {
                Cts.Dispose();
            }

            Environment.Exit((int)result);
        }

        static int ExitWithCode(ExitResult result)
        {
            return (int)result;
        }

        static HelpText GetAutoBuiltHelpText(ParserResult<object> result)
        {
            return HelpText.AutoBuild(result, h =>
            {
                h.AddOptions(result);
                return h;
            },
            e =>
            {
                return e;
            });
        }
        #endregion

        #region Event Handlers
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Error((Exception)e.ExceptionObject, "Error occurred during operation. NewsAlpha CLI will shutdown.");
            Exit(ExitResult.UNHANDLED_EXCEPTION);
        }
        #endregion
    }
}
