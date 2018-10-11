using AgainServer.Hubs;
using AgainServer.Models.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AgainServer
{
    public class Startup
    {
        public IStaticpaths path { get; set; }
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            path = new StaticPath { StatPath = env.WebRootPath };
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry(Configuration);
            services.AddMvc();
            services.AddSignalR(options =>
            {
                options.KeepAliveInterval = TimeSpan.MaxValue;
            });
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddSingleton<IStaticpaths>(path);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            // path = new StaticPath { StatPath = env.ContentRootPath };
            app.UseSignalR(route =>
            {
                route.MapHub<MessageHub>("/messageHub");
            });
            //GlobalHost.Configuration.MaxIncomingWebSocketMessageSize = null;
            app.UseMvc();

        }
    }
}