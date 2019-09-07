using System;
using Serilog;
using SerilogTimings;
using SerilogTimings.Extensions;
namespace Canaan
{
    public class SerilogLogger : Logger
    {
        public SerilogLogger(string logFileName)
        {

            Config = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(logFileName ?? "Canaan.log");
            Logger = Config.CreateLogger();
        }

        public SerilogLogger()
        {
            Config = new LoggerConfiguration()
                .WriteTo.Trace(Serilog.Events.LogEventLevel.Information);
            Logger = Config.CreateLogger();
        }

        public LoggerConfiguration Config { get; protected set; }

        public ILogger Logger { get; protected set; }

        public override void Info(string messageTemplate, params object[] args) => Logger.Information(messageTemplate, args);

        public override void Debug(string messageTemplate, params object[] args) => Logger.Debug(messageTemplate, args);

        public override void Error(string messageTemplate, params object[] args) => Logger.Error(messageTemplate, args);

        public override void Error(Exception ex, string messageTemplate, params object[] args) => Logger.Error(ex, messageTemplate, args);

        public override Op Begin(string messageTemplate, params object[] args)
        {
            Info(messageTemplate + "...", args);
            return new SerilogOp(this, Logger.BeginOperation(messageTemplate, args));
        }
    }

    public class SerilogOp : Logger.Op
    {
        public SerilogOp(SerilogLogger logger, Operation op): base(logger)
        {
            Op = op;

        }

        public override void Cancel()
        {
            Op.Cancel();
            isCancelled = true;
        }

        public override void Complete()
        {
            Op.Complete();
        }

        public override void Dispose()
        {
            if (!(isCancelled || isCompleted))
            {
                Op.Cancel();
            }
        }

        protected Operation Op;
        
    }
}
