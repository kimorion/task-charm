using Telegram.Bot;

namespace Charm.Core
{
    public static class TelegramBotClientExtensions
    {
        public static string SetupWebhook(this TelegramBotClient client, string webhookUrl)
        {
            client.SetWebhookAsync(webhookUrl).Wait();
            var info = client.GetWebhookInfoAsync();
            info.Wait();

            return info.Result.Url;
        }
    }
}