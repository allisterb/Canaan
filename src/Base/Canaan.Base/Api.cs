using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace Canaan
{
    public abstract class Api
    {
        #region Constructors
        static Api()
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CANAAN_PRODUCTION")))
            {
                Configuration = new ConfigurationBuilder()
                    .AddEnvironmentVariables()
                    .Build();
            }
            else
            {
                Configuration = new ConfigurationBuilder()
                .AddJsonFile("config.json", optional: true)
                .AddUserSecrets("81dfcf5f-a19e-4cab-a546-9fa5b09927b8")
                .Build();
            }

            HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Canaan/0.1");
        }
        public Api(CancellationToken ct)
        {
            if (Logger == null)
            {
                throw new InvalidOperationException("A logger is not assigned.");
            }
            CancellationToken = ct;
            Type = this.GetType();
        }
        public Api(): this(Cts.Token) {}

        #endregion

        #region Properties
        public static IConfigurationRoot Configuration { get; protected set; }

        public static Logger Logger { get; protected set; }

        public static CancellationTokenSource Cts { get; } = new CancellationTokenSource();

        public static HttpClient HttpClient { get; } = new HttpClient();

        public static string YY = DateTime.Now.Year.ToString().Substring(2, 2);

        public bool Initialized { get; protected set; }

        public static bool IsAzureFunction { get; set; }

        public CancellationToken CancellationToken { get; protected set; }

        public Type Type { get; }

        #endregion

        #region Methods
        public static void SetLogger(Logger logger)
        {
            Logger = logger;
        }

        public static void SetLoggerIfNone(Logger logger)
        {
            if (Logger == null)
            {
                Logger = logger;
            }
        }

        public static void SetDefaultLoggerIfNone()
        {
            if (Logger == null)
            {
                Logger = new ConsoleLogger();
            }
        }

        public static string Config(string i)
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CANAAN_PRODUCTION")) || IsAzureFunction)
            {
                i = i.Replace(":", "_");
                return Api.Configuration[i];
            }
            else
            {
                return Api.Configuration[i];
            }
            
        }

        public static void Info(string messageTemplate, params object[] args) => Logger.Info(messageTemplate, args);

        public static void Debug(string messageTemplate, params object[] args) => Logger.Debug(messageTemplate, args);

        public static void Error(string messageTemplate, params object[] args) => Logger.Error(messageTemplate, args);

        public static void Error(Exception ex, string messageTemplate, params object[] args) => Logger.Error(ex, messageTemplate, args);

        public static Logger.Op Begin(string messageTemplate, params object[] args) => Logger.Begin(messageTemplate, args);

        public void ThrowIfNotInitialized()
        {
            if (!this.Initialized) throw new ApiNotInitializedException(this);
        }
        #endregion
    }
}
