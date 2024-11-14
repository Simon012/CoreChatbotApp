// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Bot.Connector.Authentication;
using System;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Bot.Builder.ApplicationInsights;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.Core;
using CoreChatbotApp.Logic;
using CoreChatbotApp.Infrastructure.DataLayer;
using CoreChatbotApp.Utilities.Configurations;
using CoreChatbotApp.Utilities.Middleware;
using CoreChatbotApp.Utilities.Telemetry;
using CoreChatbotApp.Infrastructure.Clients.SharePoint;
using CoreChatbotApp.Infrastructure.Clients.Timelog;
using CoreChatbotApp.Infrastructure.DataLayer.Cache;
using CoreChatbotApp.Utilities.Configurations.Timelog;
using CoreChatbotApp.Infrastructure.DataLayer.Cache.Redis;

namespace CoreChatbotApp
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
            services.AddHttpClient().AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.MaxDepth = HttpHelper.BotMessageSerializerSettings.MaxDepth;
            });

            // The following four services are standard and not application-specific
            // Use ConfigurationBotFrameworkAuthentication which will pick up MicrosoftAppId and MicrosoftAppPassword from configuration
            services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();
            // We need the following two lines to be able to proactively message the user
            services.AddSingleton<BotAdapter, AdapterWithErrorHandler>();
            services.AddSingleton(sp => (IBotFrameworkHttpAdapter)sp.GetRequiredService<BotAdapter>());
            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, CoreChatBot>();


            // The now following services are application-specific
            // Telemetry / Logging stuff
            services.AddApplicationInsightsTelemetry();// Add Application Insights services into service collection
            services.AddApplicationInsightsTelemetryProcessor<TelemetryProcessor>();// Add the telemetry processor.
            services.AddSingleton<IBotTelemetryClient, BotTelemetryClient>();// Create the telemetry client. Reactivate for production!!!!!!!!!!!
            services.AddSingleton<ITelemetryInitializer, OperationCorrelationTelemetryInitializer>();// Add telemetry initializer that will set the correlation context for all telemetry items.
            services.AddSingleton<ITelemetryInitializer, TelemetryBotIdInitializer>();// Add telemetry initializer that sets the user ID and session ID (in addition to other bot-specific properties such as activity ID)
            services.AddSingleton<TelemetryInitializerMiddleware>();// Create the telemetry middleware to initialize telemetry gathering
            services.AddSingleton<TelemetryLoggerMiddleware>();// Create the telemetry middleware (used by the telemetry initializer) to track conversation events

            // For the conversation lock middleware
            services.AddScoped<IMiddleware, ConversationLockMiddleware>();

            // Configs:
            services.Configure<TimelogTenantsConfig>(Configuration.GetSection("TimelogTenants"));
            services.Configure<ApplicationConfig>(Configuration);
            services.Configure<CacheConfig>(Configuration);

            // Data Layer:
            services.AddSingleton<IRedisCacheConnection, RedisCacheConnection>();
            services.AddSingleton<ICacheManager, RedisCacheManager>();
            services.AddSingleton<IDataManager, DataManager>();

            // For Timelog-Access:
            services.AddHttpClient<TimelogClient>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10); // Adjust as needed
                client.MaxResponseContentBufferSize = 10 * 1024 * 1024; // 10 MB
            });
            services.AddSingleton<SharePointClient>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            // app.UseHttpsRedirection();
        }
    }
}
