using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace Charm.Core
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            var botSettings = new ConfigurationBuilder()
                .AddJsonFile("./ssl/telegram-bot-settings.json", optional: false)
                .Build();
            TelegramSettingsSection = botSettings.GetSection("TelegramBotSettings");
        }

        private IConfiguration Configuration { get; }
        private IConfigurationSection TelegramSettingsSection { get; }
        private string setWebhookUrl;


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();
            services.AddTransient<ITelegramBotClient, TelegramBotClient>(provider =>
            {
                var telegramClient = new TelegramBotClient(TelegramSettingsSection["Token"]);
                return telegramClient;
            });

            setWebhookUrl =
                new TelegramBotClient(TelegramSettingsSection["Token"])
                    .SetupWebhook(TelegramSettingsSection["WebhookUrl"]);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            logger.LogInformation($"Telegram Webhook set: {setWebhookUrl}");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}