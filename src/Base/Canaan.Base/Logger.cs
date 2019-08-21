using System;

namespace Canaan
{
    public abstract class Logger
    {
        public bool IsConfigured { get; protected set; } = false;

        public abstract void Info(string messageTemplate, params object[] args);

        public abstract void Debug(string messageTemplate, params object[] args);

        public abstract void Error(string messageTemplate, params object[] args);

        public abstract void Error(Exception ex, string messageTemplate, params object[] args);
    }

    public class ConsoleLogger : Logger
    {
        public override void Info(string messageTemplate, params object[] args) => Console.WriteLine(messageTemplate, args);

        public override void Debug(string messageTemplate, params object[] args) => Console.WriteLine(messageTemplate, args);

        public override void Error(string messageTemplate, params object[] args) => Console.WriteLine(messageTemplate, args);

        public override void Error(Exception ex, string messageTemplate, params object[] args) => Console.WriteLine(messageTemplate, args);
    }
}
