using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

namespace Charm.Application
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("./conf/telegram-bot-settings.json", optional: false)
                .Build();
            var telegramBotSettings = config.GetSection("TelegramBotSettings");

            CreateHostBuilder(args).Build().Run();

            var client = new TelegramBotClient(telegramBotSettings["Token"]);
            client.DeleteWebhookAsync().Wait();
            Console.WriteLine("Webhook removed");
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    // webBuilder.ConfigureKestrel(serverOptions =>
                    // {
                    //     serverOptions.ConfigureHttpsDefaults(listenOptions =>
                    //     {
                    //         var files = Directory.GetFiles("./ssl");
                    //         listenOptions.ServerCertificate = new X509Certificate2("./ssl/CHARMPUBLIC.pem");
                    //     });
                    // });
                    webBuilder.UseStartup<Startup>();
                });
    }
}