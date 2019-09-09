using System;
using System.Collections.Generic;
using System.Text;

using CommandLine;
using CommandLine.Text;

namespace NewsAlpha
{
    class SocialNewsOptions
    {
        [Option('s', "search", Required = true, HelpText = "Search social media posts for the specified text.")]
        public string Search { get; set; }

        [Option('i', "identity", Required = false, Default = false, HelpText = "Filter on social media posts containing identity hate.")]
        public bool IdentityHate { get; set; }

        [Option('l', "links", Default = false, Required = false, HelpText = "Search social media posts for links with the specified texts.")]
        public bool Links { get; set; }

        [Option('t', "count", Default = 0, Required = false, HelpText = "Limit search results to the specified number.")]
        public int Count { get; set; }
    }

    [Verb("gab", HelpText  = "Query Gab social news posts.")]
    class GabOptions : SocialNewsOptions
    {

    }

    [Verb("4chpol", HelpText = "Query 4chan /pol/ social news posts.")]
    class FourChanPolOptions : SocialNewsOptions
    {
     

    }

    [Verb("articles", HelpText = "Create and query news article data.")]
    class ArticleOptions
    {
        [Option('a', "add", Required = false, HelpText = "Add a topic to index articles for.")]
        public string AddTopic { get; set; }

        [Option('c', "count", Required = false, HelpText = "Limit news articles to the specified number.")]
        public int Count { get; set; }

        [Option('r', "retrieve", Required = false, HelpText = "Retrieve article index data from a topic.")]
        public string RetrieveTopic { get; set; }
    }

}
