using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Charm.Core.Infrastructure;
using Charm.Core.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Charm.Application
{
    public class CharmNotifierService
    {
        private readonly ILogger _logger;
        private readonly CharmDbContext _context;
        private readonly ITelegramBotClient _client;

        public CharmNotifierService(
            ILogger<CharmNotifierService> logger,
            CharmDbContext context,
            ITelegramBotClient client)
        {
            _logger = logger;
            _context = context;
            _client = client;
        }

        public async Task DoWork(CancellationToken stoppingToken)
        {
            const int maxUsersCount = 25;

            while (!stoppingToken.IsCancellationRequested)
            {
                List<Reminder> expiredReminders = await GetExpiredReminders(maxUsersCount);

                await SendNotificationsAsync(expiredReminders);
                await RemoveExpiredReminders(expiredReminders);

                await WaitCooldown();
            }
        }

        private async Task<List<Reminder>> GetExpiredReminders(int count)
        {
            var currentDateTimeOffset = DateTimeOffset.Now;
            return await _context.Reminders
                .Where(e => e.Deadline <= currentDateTimeOffset)
                .OrderByDescending(e => e.Deadline)
                .Take(count)
                .Include(e => e.Gist)
                .ThenInclude(e => e.User)
                .AsNoTracking()
                .ToListAsync();
        }

        private async Task SendNotificationsAsync(List<Reminder> reminders)
        {
            var sentCount = 0;

            foreach (var reminder in reminders)
            {
                var text = GenerateNotificationText(reminder);

                try
                {
                    await _client.SendTextMessageAsync(reminder.Gist.UserId.ToString(), text, ParseMode.Html);
                    sentCount++;
                }
                catch (Exception e)
                {
                    _logger.LogCritical("Couldn't send notification: " + e.Message);
                }
            }

            if (sentCount == 0)
            {
                _logger.LogDebug($"{sentCount} notifications was sent ({DateTime.Now:T})");
            }
            else
            {
                _logger.LogInformation($"{sentCount} notifications was sent ({DateTime.Now:T})");
            }
        }

        private static string GenerateNotificationText(Reminder reminder)
        {
            StringBuilder builder = new StringBuilder();
            var eventTime = reminder.Deadline;
            if (reminder.Advance is not null) eventTime += reminder.Advance.Value;
            var dateTimeString = eventTime.ToString("yyyy-M-d dddd HH:mm", CultureInfo.GetCultureInfo("RU-ru"));
            dateTimeString = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(dateTimeString);

            builder.AppendLine("<b>Напоминание:</b> ");
            builder.AppendLine();
            builder.Append("<i>");
            builder.AppendLine(reminder.Gist.Text);
            builder.Append("</i>");
            builder.AppendLine();
            builder.Append("<u>");
            builder.AppendLine($"{dateTimeString}");
            builder.Append("</u>");

            return builder.ToString();
        }

        private Task RemoveExpiredReminders(List<Reminder> reminders)
        {
            var entities = reminders.Select(e => new Reminder {Id = e.Id}).ToList();
            _context.Reminders.RemoveRange(entities);
            return _context.SaveChangesAsync();
        }

        private static Task WaitCooldown()
        {
            return Task.Delay(2000);
        }
    }

    public class CharmNotifierHostedService : BackgroundService
    {
        private readonly ILogger<CharmNotifierHostedService> _logger;
        private readonly IServiceProvider _services;

        public CharmNotifierHostedService(
            IServiceProvider services,
            ILogger<CharmNotifierHostedService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting notifier service...");
            await DoWork(stoppingToken);
            _logger.LogInformation("Stopping notifier service...");
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            using var scope = _services.CreateScope();
            var scopedProcessingService =
                scope.ServiceProvider
                    .GetRequiredService<CharmNotifierService>();

            await scopedProcessingService.DoWork(stoppingToken);
        }
    }
}