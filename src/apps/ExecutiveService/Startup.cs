using System;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Hypergiant.HIVE.HGSExecutive
{
    public class ExecutiveProvider
    {
        public ICommandExecutive CurrentExecutive { get; set; }
        public StorageProvider StorageProvider { get; }

        public ExecutiveProvider(StorageProvider storageProvider)
        {
            StorageProvider = storageProvider;
        }

        public void ConfigureExecutive(ExecutiveConfiguration config)
        {
            StorageProvider.ServicePort = config.StorageServicePort;

            switch (config.ExecutiveType.ToLower())
            {
                case "cfdp":
                    CurrentExecutive = new CfdpCommandExecutive(StorageProvider, config.SatelliteAddress, config.DestinationUplinkFolder, config.CfdpConfigFile, config.SatelliteCfdpEntityID);
                    break;
                case "scp":
                    CurrentExecutive = new ScpCommandExecutive(StorageProvider, config.SatelliteAddress, config.DestinationUplinkFolder, config.UserName, config.Password);
                    break;
                default:
                    throw new NotSupportedException($"Executive type '{config.ExecutiveType}' not supported.");
            }
        }
    }

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<StorageProvider>();
            services.AddSingleton<ExecutiveProvider>();

            services.AddControllers()
             .AddJsonOptions(options =>
             {
                 options.JsonSerializerOptions.IgnoreNullValues = true;
                 options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
             });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
