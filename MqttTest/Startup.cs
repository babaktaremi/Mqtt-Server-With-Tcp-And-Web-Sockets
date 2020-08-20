using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet.AspNetCore;
using MQTTnet.AspNetCore.Extensions;
using MqttTest.Workers;

namespace MqttTest
{
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
            #region Mqtt Web Socket

            services
                .AddHostedMqttServer(mqttServer =>
                {
                    mqttServer.WithDefaultEndpoint();
                    mqttServer.WithPersistentSessions();
                    mqttServer.Build();
                    mqttServer.WithDefaultCommunicationTimeout(TimeSpan.FromMinutes(10));
                })
                .AddMqttConnectionHandler()
                .AddWebSockets(options =>
                {
                    options.KeepAliveInterval = TimeSpan.FromMinutes(15);
                })
                .AddConnections();

            #endregion

            #region Mqtt Service Worker Tcp

            services.AddHostedService<MqttServerWorker>();

            #endregion

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapMqtt("/mqtt");
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Mqtt Server Is Running");
                });
            });
        }
    }
}
