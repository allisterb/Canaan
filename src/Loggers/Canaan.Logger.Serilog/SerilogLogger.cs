using System;
using Serilog;
using Serilog.Core;

namespace Canaan
{
    public class SerilogLogger : Logger
    {
        public SerilogLogger(string logFileName = null)
        {
            Config = new LoggerConfiguration().WriteTo.File(logFileName ?? "Canaan.log");
            Logger = Config.CreateLogger();
        }

        public LoggerConfiguration Config { get; protected set; }

        public ILogger Logger { get; protected set; }

        public override void Info(string messageTemplate, params object[] args) => Logger.Information(messageTemplate, args);

        public override void Debug(string messageTemplate, params object[] args) => Logger.Debug(messageTemplate, args);

        public override void Error(string messageTemplate, params object[] args) => Logger.Error(messageTemplate, args);

        public override void Error(Exception ex, string messageTemplate, params object[] args) => Logger.Error(ex, messageTemplate, args);
    }
}
