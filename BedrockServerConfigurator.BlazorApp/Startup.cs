using System;
using System.IO;
using BedrockServerConfigurator.BlazorApp.Data;
using BedrockServerConfigurator.Library;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BedrockServerConfigurator.BlazorApp
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        private readonly Configurator _configurator;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            string defaultPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

            var defaultServersPath = Path.Combine(defaultPath, "bedrockServers");
            var defaultServerName = "bedServer";

            var serversPath = Configuration.GetValue<string>("ServersPath");
            var serverName = Configuration.GetValue<string>("ServerName");

            if (serversPath == "")
            {
                serversPath = defaultServersPath;
            }

            if (serverName == "")
            {
                serverName = defaultServerName;
            }

            _configurator = Configurator.CreateInstance(serversPath, serverName);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();

            services.AddSingleton(_configurator);
            services.AddSingleton(new ConfiguratorData());
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
