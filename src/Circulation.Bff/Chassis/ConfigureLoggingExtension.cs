using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using Serilog.Formatting.Compact;
using SumoLogic.Logging.Serilog.Extensions;

namespace Circulation.Bff.Chassis
{
    public static class ConfigureLoggingExtension
    {
        public static LoggerConfiguration ConfigureLogging(this LoggerConfiguration loggerConfiguration,
            IConfiguration appConfiguration)
        {
            var localLoggingConfiguration = new LocalLoggingConfiguration();
            appConfiguration.Bind("Logging", localLoggingConfiguration);

            loggerConfiguration.Enrich.FromLogContext();
            //TODO(generator): add appropriate enrichers

            ConfigureLogLevels(loggerConfiguration, localLoggingConfiguration);

            ConfigureConsoleSink(loggerConfiguration, localLoggingConfiguration);

            ConfigureFileSink(loggerConfiguration, localLoggingConfiguration);

            ConfigureSumoLogicSink(loggerConfiguration, localLoggingConfiguration);

            loggerConfiguration.WriteTo.ApplicationInsights(TelemetryConfiguration.Active, TelemetryConverter.Traces);

            return loggerConfiguration;
        }

        private static void ConfigureLogLevels(LoggerConfiguration loggerConfiguration,
            LocalLoggingConfiguration localLoggingConfiguration)
        {
            if (!string.IsNullOrEmpty(localLoggingConfiguration.MinLevel))
            {
                loggerConfiguration.MinimumLevel.Is(Enum.Parse<LogEventLevel>(localLoggingConfiguration.MinLevel, true));
            }
            else
            {
                loggerConfiguration.MinimumLevel.Is(LogEventLevel.Verbose);
            }

            // now do the log level overrides
            foreach (var pair in localLoggingConfiguration.MinLevelPerSource)
            {
                var level = Enum.Parse<LogEventLevel>(pair.Value, true);
                loggerConfiguration.MinimumLevel.Override(pair.Key, level);
            }
        }

        private static void ConfigureConsoleSink(LoggerConfiguration loggerConfiguration,
            LocalLoggingConfiguration localLoggingConfiguration)
        {
            if (localLoggingConfiguration.Console?.Enable ?? false)
            {
                var minLevelForConsole = LogEventLevel.Verbose;
                if (!string.IsNullOrEmpty(localLoggingConfiguration.Console.MinLevel))
                {
                    minLevelForConsole = Enum.Parse<LogEventLevel>(localLoggingConfiguration.Console.MinLevel, true);
                }

                loggerConfiguration.WriteTo.Console(
                    restrictedToMinimumLevel: minLevelForConsole,
                    standardErrorFromLevel: LogEventLevel.Warning
                );
            }
        }

        private static void ConfigureFileSink(LoggerConfiguration loggerConfiguration,
            LocalLoggingConfiguration localLoggingConfiguration)
        {
            if (localLoggingConfiguration.File?.Enable ?? false)
            {
                var fileConfig = localLoggingConfiguration.File;

                // rolling file size:
                // not specified: use default of 1GB
                // specified null: use default of 1GB
                // specified value: the value
                // NOTE: this code eliminates the possibility of disabling rolling by file size
                int fileSizeLimitMegabytes = fileConfig.RollingMB.GetValueOrDefault(1_024);

                var minLevelForFile = LogEventLevel.Verbose;
                if (!string.IsNullOrEmpty(fileConfig.MinLevel))
                {
                    minLevelForFile = Enum.Parse<LogEventLevel>(fileConfig.MinLevel, true);
                }

                // if not specified, specified with null or specified with empty string, disable interval-based rolling.
                var rollingInterval = RollingInterval.Infinite;
                if (!string.IsNullOrEmpty(fileConfig.RollingInterval))
                {
                    rollingInterval = Enum.Parse<RollingInterval>(fileConfig.RollingInterval, true);
                }

                loggerConfiguration.WriteTo.File(
                    localLoggingConfiguration.File.Path,
                    fileSizeLimitBytes: fileSizeLimitMegabytes * 1_024L * 1_024L,
                    restrictedToMinimumLevel: minLevelForFile,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    formatProvider: null,
                    levelSwitch: null,
                    buffered: fileConfig.Buffered, //NOTE: unbuffered logging to the file trades-off performance for immediate log output.
                    shared: false,
                    flushToDiskInterval: null, // probably only applies if buffered is true
                    rollingInterval: rollingInterval,
                    rollOnFileSizeLimit: true, // false is the default, but we prefer to roll over to another file than stop logging altogether.
                    //TODO: allow the number of retained files to be configured.
                    retainedFileCountLimit: 31,
                    encoding: null // defaults to UTF-8 without BOM
                );
            }
        }

        private static void ConfigureSumoLogicSink(LoggerConfiguration loggerConfiguration,
            LocalLoggingConfiguration localLoggingConfiguration)
        {
            // See https://github.com/SumoLogic/sumologic-net-appenders/blob/master/docs/sumologic.logging.serilog.md
            // for configuration docs
            var sumoLogicConfig = localLoggingConfiguration.SumoLogic;
            if (sumoLogicConfig?.Enable ?? false)
            {
                var minLevelForSumo = LogEventLevel.Verbose;
                if (!string.IsNullOrEmpty(sumoLogicConfig.MinLevel))
                {
                    minLevelForSumo = Enum.Parse<LogEventLevel>(sumoLogicConfig.MinLevel, true);
                }

                if(sumoLogicConfig.Buffered)
                {
                    loggerConfiguration.WriteTo.BufferedSumoLogic(
                        new Uri(sumoLogicConfig.EndpointUrl),
                        formatter: new CompactJsonFormatter(),
                        restrictedToMinimumLevel: minLevelForSumo,
                        sourceName: sumoLogicConfig.SourceName,
                        sourceCategory: sumoLogicConfig.SourceCategory,
                        clientName: sumoLogicConfig.ClientName
                        //TODO: pass buffering configuration parameters
                    );
                }
                else
                {
                    loggerConfiguration.WriteTo.SumoLogic(
                        new Uri(sumoLogicConfig.EndpointUrl),
                        formatter: new CompactJsonFormatter(),
                        restrictedToMinimumLevel: minLevelForSumo,
                        sourceName: sumoLogicConfig.SourceName,
                        sourceCategory: sumoLogicConfig.SourceCategory,
                        clientName: sumoLogicConfig.ClientName);
                }
            }
        }
    }

    public class LocalLoggingConfiguration
    {
        public string MinLevel { get; set; }

        public Dictionary<string,string> MinLevelPerSource { get; set; }

        public SumoLogicConfig SumoLogic { get; set; }

        public ConsoleConfig Console { get; set; }

        public FileConfig File { get; set; }

        public class SumoLogicConfig
        {
            public bool Enable { get; set; }

            public string MinLevel { get; set; }

            public string EndpointUrl { get; set; }

            public string SourceName { get; set; }

            public string SourceCategory { get; set; }

            public string ClientName { get; set; }

            public bool Buffered { get; set; }
        }

        public class ConsoleConfig
        {
            public bool Enable { get; set; }

            public string MinLevel { get; set; }
        }

        public class FileConfig
        {
            public bool Enable { get; set; }

            public string Path { get; set; }

            public string RollingInterval { get; set; }

            public int? RollingMB { get; set; }

            public bool Buffered { get; set; }

            public string MinLevel { get; set; }
        }
    }

}
