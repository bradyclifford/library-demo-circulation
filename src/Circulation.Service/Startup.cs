using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Circulation.Infrastructure;
using Circulation.Service.Chassis;
using Circulation.Service.HealthChecks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Circulation.Service
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

            services.AddSqlConnection(Configuration["SQL_CONN_STR"]);

            services.AddInfrastructureImplementations(Configuration["EVENTHUB_CONN_STR"]);
            services.AddDomainServices();

            services.AddHealthChecks()
                .AddCheck<LivenessCheck>("Liveness", HealthStatus.Unhealthy)
                //TODO: add relevant health checks
                ;

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            //TODO(generator): configure bearer token authentication
        }

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
        }
    }
}
