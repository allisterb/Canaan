using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Azure.WebJobs.Host;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;

using Serilog.Configuration;
using Serilog.Formatting.Display;

namespace NewsAlpha
{
    public class TraceWriterSink : ILogEventSink
    {
        private readonly TraceWriter m_traceWriter;
        private readonly ITextFormatter m_formatter;

        /// <summary>
        /// Builds a sink that can link to an Azure WebJob TraceWriter
        /// </summary>
        /// <param name="traceWriter">The trace writer to log to</param>
        /// <param name="formatter">The formatter to use on emit</param>
        public TraceWriterSink(TraceWriter traceWriter, ITextFormatter formatter)
        {
            if (traceWriter == null)
            {
                throw new ArgumentNullException(nameof(traceWriter));
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            m_traceWriter = traceWriter;
            m_formatter = formatter;
        }

        /// <summary>
        /// Emits an event to the underlying tracewriter
        /// </summary>
        /// <param name="logEvent">The log event to emit</param>
        public void Emit(LogEvent logEvent)
        {
            if (logEvent == null)
            {
                return;
            }

            TraceEvent traceEvent = BuildTraceEvent(logEvent, m_formatter);

            m_traceWriter.Trace(traceEvent);
        }

        internal static TraceEvent BuildTraceEvent(LogEvent logEvent, ITextFormatter formatter)
        {
            if (logEvent == null || formatter == null)
            {
                return null;
            }

            string message = FormatLogEventMessage(logEvent, formatter);

            TraceLevel traceLevel = GetLogEventTraceLevel(logEvent.Level);

            string source = GetLogEventSourceProperty(logEvent.Properties);

            return new TraceEvent(traceLevel, message, source, logEvent.Exception);
        }

        internal static string GetLogEventSourceProperty(IReadOnlyDictionary<string, LogEventPropertyValue> logEventProperties)
        {
            if (logEventProperties == null ||
                !logEventProperties.ContainsKey(Constants.SourceContextPropertyName))
            {
                return null;
            }

            ScalarValue sourceValue = logEventProperties[Constants.SourceContextPropertyName] as ScalarValue;

            return sourceValue?.Value?.ToString();
        }

        internal static TraceLevel GetLogEventTraceLevel(LogEventLevel logEventLevel)
        {
            if (logEventLevel == LogEventLevel.Fatal || logEventLevel == LogEventLevel.Error)
            {
                return TraceLevel.Error;
            }

            if (logEventLevel == LogEventLevel.Warning)
            {
                return TraceLevel.Warning;
            }

            if (logEventLevel == LogEventLevel.Information)
            {
                return TraceLevel.Info;
            }

            return TraceLevel.Verbose;
        }

        internal static string FormatLogEventMessage(LogEvent logEvent, ITextFormatter formatter)
        {
            if (logEvent == null || formatter == null)
            {
                return null;
            }

            using (StringWriter render = new StringWriter())
            {
                formatter.Format(logEvent, render);

                return render.ToString();
            }
        }
    }

    public static class TraceWriterLoggerConfigurationExtensions
    {
        private const string DefaultOutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}";

        public static LoggerConfiguration TraceWriter(
            this LoggerSinkConfiguration loggerConfiguration,
            TraceWriter traceWriter,
            string outputTemplate = DefaultOutputTemplate,
            IFormatProvider formatProvider = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum)
        {
            if (loggerConfiguration == null)
            {
                throw new ArgumentNullException(nameof(loggerConfiguration));
            }

            if (traceWriter == null)
            {
                throw new ArgumentNullException(nameof(traceWriter));
            }

            if (outputTemplate == null)
            {
                throw new ArgumentNullException(nameof(outputTemplate));
            }

            ITextFormatter formatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);

            return TraceWriter(loggerConfiguration, traceWriter, formatter, restrictedToMinimumLevel);
        }

        public static LoggerConfiguration TraceWriter(
            this LoggerSinkConfiguration loggerConfiguration,
            TraceWriter traceWriter,
            ITextFormatter formatter,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum)
        {
            if (loggerConfiguration == null)
            {
                throw new ArgumentNullException(nameof(loggerConfiguration));
            }

            if (traceWriter == null)
            {
                throw new ArgumentNullException(nameof(traceWriter));
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            TraceWriterSink sink = new TraceWriterSink(traceWriter, formatter);

            return loggerConfiguration.Sink(sink, restrictedToMinimumLevel);
        }
    }

}