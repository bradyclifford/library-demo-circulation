using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Circulation.Bff.Chassis;

namespace Circulation.Bff
{
    public class Program
    {
        public static IConfiguration Configuration { get; private set; }

        public static int Main(string[] args)
        {
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                // This does not support environment-specific configuration files.
                // Instead, per-environment logging configuration should be accomplished by
                // modifying appsettings.json during packaging or releasing.
                .AddEnvironmentVariables();

            Configuration = configBuilder.Build();

            //TODO: Output to somewhere more useful than the console in deployed environments.
            Serilog.Debugging.SelfLog.Enable(Console.Error);

            // In order to be able to log information throughout the lifecycle
            // of this process, including while attempting to bootstrap the
            // WebHost, we're configuring the logger here instead of in
            // Startup.cs
            Log.Logger = new LoggerConfiguration()
                .ConfigureLogging(Configuration)
                .CreateLogger();

            var programlogger = Log.ForContext<Program>();
            programlogger.Verbose("Building and running WebHost");

            try
            {
                CreateWebHostBuilder(args).Build().Run();
                programlogger.Verbose("WebHost exiting normally");
            }
            catch (Exception ex)
            {
                programlogger.Fatal(ex, "WebHost terminated unexpectedly.");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }

            return 0;
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                // Leaving the web root directory unspecified defaults to this same value ("wwwroot").
                // However, just letting it default means that if the directory doesn't exist
                // _at startup time_,  the Static Files middleware won't serve any files at all! By
                // specifying it, the directory will be created on startup if necessary and files
                // that appear in it later will be served. This is useful during local development
                // because webpack can be putting new files in there.
                .UseWebRoot("wwwroot")
                .UseConfiguration(Configuration)
                .UseApplicationInsights()
                .UseSerilog();
    }
}
