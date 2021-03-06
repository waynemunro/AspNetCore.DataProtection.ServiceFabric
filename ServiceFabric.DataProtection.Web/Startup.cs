﻿using AspNetCore.DataProtection.ServiceFabric.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;
using System.Fabric;

namespace ServiceFabric.DataProtection.Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            string dataProtectionServiceUri = string.Empty;
            try
            {
                dataProtectionServiceUri = $"{FabricRuntime.GetActivationContext().ApplicationName}/DataProtectionService";
            }
            catch
            {
                dataProtectionServiceUri = "fabric:/ServiceFabric.DataProtection/DataProtectionService";
            }

            // Add Service Fabric DataProtection
            services.AddDataProtection()
                    .SetApplicationName("ServiceFabric-DataProtection-Web")
                    .PersistKeysToServiceFabric(dataProtectionServiceUri);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "AspNetCore.DataProtection.ServiceFabric API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            //loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            //loggerFactory.AddDebug();

            app.UseMvc();

            app.UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((swagger, httpReq) => swagger.Host = httpReq.Host.Value);
            });

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "AspNetCore.DataProtection.ServiceFabric API V1 Docs");
            });
           
        }
    }
}
