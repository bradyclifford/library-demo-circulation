using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Circulation.Bff.Chassis;
using Circulation.Bff.HealthChecks;
using Circulation.Infrastructure;

namespace Circulation.Bff
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }

        public IHostingEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
#if DEBUG
            if (Environment.IsDevelopment())
            {
                StartWebpackDevServer(Path.Join(Environment.ContentRootPath, "FrontEnd"));
            }
#endif

            services.AddSqlConnection(Configuration["SQL_CONN_STR"]);

            services.AddInfrastructureImplementations(Configuration["EVENTHUB_CONN_STR"]);
            services.AddDomainServices();

            services.AddHealthChecks()
                .AddCheck<LivenessCheck>("Liveness", HealthStatus.Unhealthy)
                //TODO: add relevant health checks
                ;

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            //TODO(generator): configure cookie authentication (for BFFs).
            //If this template gets reused for services, configure bearer token instead.
        }

#if DEBUG
        private void StartWebpackDevServer(string projectPath)
        {
            var startinfo = new ProcessStartInfo("node")
            {
                WorkingDirectory = projectPath,
                Arguments = $"devServer.js {Process.GetCurrentProcess().Id}",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            var webPackDevServer = Process.Start(startinfo);
            webPackDevServer.OutputDataReceived += (sender, args) => { Console.Out.WriteLine(args.Data); };
            webPackDevServer.ErrorDataReceived += (sender, args) => { Console.Error.WriteLine(args.Data); };
            webPackDevServer.BeginOutputReadLine();
            webPackDevServer.BeginErrorReadLine();
        }
#endif

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UsePathBase("/circulation");
            //TODO(generator): include middleware for
            // * Exception handling/logging
            // * request tracing/correlation id
            // * cookie authentication

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHealthChecks("/health");

            app.UseMvc();

            // Serve static files and default documents from the wwwroot directory
            app.UseFileServer();

            // Rewrite all remaining GET requests to serve index.html in support of client-side-routed URL paths.
            // NOTE: Using these two lines instead of .UseSpa() to avoid unhandled exceptions on
            // unsupported request methods: https://github.com/aspnet/AspNetCore/issues/5223#issuecomment-433394061
            app.Use((context, next) =>
            {
                if(context.Request.Method == HttpMethods.Get)
                    context.Request.Path = "/index.html";
                return next();
            });
            app.UseStaticFiles();
        }
    }
}
