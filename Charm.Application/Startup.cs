using System;
using Charm.Core.Domain;
using Charm.Core.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace Charm.Application
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            var botSettings = new ConfigurationBuilder()
                .AddJsonFile("./conf/telegram-bot-settings.json", optional: false)
                .Build();
            TelegramSettingsSection = botSettings.GetSection("TelegramBotSettings");

            var dbSettings = new ConfigurationBuilder()
                .AddJsonFile("./conf/database-settings.json", optional: false)
                .Build();
            DbSettingsSection = dbSettings.GetSection("ConnectionStrings");
        }

        private IConfiguration Configuration { get; }
        private IConfigurationSection TelegramSettingsSection { get; }
        private IConfigurationSection DbSettingsSection { get; }
        private string setWebhookUrl;


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<CharmDbContext>(
                options => options.UseNpgsql(DbSettingsSection["Main"]));
            services.AddTransient<CharmInterpreter>();
            services.AddTransient<CharmLibrarian>();

            services.AddControllers().AddNewtonsoftJson();
            services.AddTransient<ITelegramBotClient, TelegramBotClient>(provider =>
            {
                var telegramClient = new TelegramBotClient(TelegramSettingsSection["Token"]);
                return telegramClient;
            });

            setWebhookUrl =
                new TelegramBotClient(TelegramSettingsSection["Token"])
                    .SetupWebhook(TelegramSettingsSection["WebhookUrl"]);

            services.AddSwaggerGen();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            logger.LogInformation($"Telegram Webhook set: {setWebhookUrl}");

            UpdateDatabase(app);

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = string.Empty;
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }

        private static void UpdateDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<CharmDbContext>())
                {
                    context.Database.Migrate();
                }
            }
        }
    }
}