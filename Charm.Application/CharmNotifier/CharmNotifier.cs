using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Charm.Core.Infrastructure;
using Charm.Core.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace Charm.Application.CharmNotifier
{
    public class CharmNotifier
    {
        private readonly CharmDbContext _context;
        private readonly TelegramBotClient _client;
        private readonly ILogger<CharmNotifier> _logger;

        public CharmNotifier(TelegramBotClient client, CharmDbContext context, ILogger<CharmNotifier> logger)
        {
            _client = client;
            _context = context;
            _logger = logger;
        }

        public Task StartNotifier()
        {
            return Send();
        }

        public async Task Send(int maxUsersCount = 25)
        {
            while (true)
            {
                List<Reminder> expiredReminders = await GetExpiredReminders(maxUsersCount);

                await SendNotificationsAsync(expiredReminders);
                await RemoveExpiredReminders(expiredReminders);

                await WaitCooldown();
            }
        }

        private async Task<List<Reminder>> GetExpiredReminders(int count)
        {
            return await _context.Reminders
                .Include(e => e.Gist)
                .ThenInclude(e => e.User)
                .Where(e => e.Deadline >= DateTimeOffset.Now)
                .OrderByDescending(e => e.Deadline)
                .Take(count)
                .AsNoTracking()
                .ToListAsync();
        }

        private async Task SendNotificationsAsync(List<Reminder> reminders)
        {
            var sentCount = 0;

            foreach (var reminder in reminders)
            {
                var text = GenerateNotificationTest(reminder);

                try
                {
                    await _client.SendTextMessageAsync(reminder.Gist.UserId.ToString(), text);
                    sentCount++;
                }
                catch (Exception e)
                {
                    _logger.LogCritical("Couldn't send notification: " + e.Message);
                }
            }

            _logger.LogInformation($"{sentCount} notifications was sent");
        }

        private static string GenerateNotificationTest(Reminder reminder)
        {
            StringBuilder builder = new StringBuilder();
            var eventTime = reminder.Deadline;
            if (reminder.Advance is not null) eventTime += reminder.Advance.Value;

            builder.AppendLine("**Напоминание:** ");
            builder.AppendLine();
            builder.AppendLine($"__({eventTime:t dd MMMM yyyy})__");
            builder.AppendLine();
            builder.AppendLine(reminder.Gist.Text);

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
}