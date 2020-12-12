using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Charm.Core.Domain.Dto;
using Charm.Core.Infrastructure;
using Charm.Core.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Charm.Core.Domain.Services
{
    public class CharmManager
    {
        public readonly CharmDbContext Context;
        private readonly ILogger<CharmManager> _logger;
        private readonly UserService _userService;
        private readonly ITelegramBotClient _client;


        public CharmManager(
            CharmDbContext context,
            ILogger<CharmManager> logger,
            UserService userService,
            ITelegramBotClient client)
        {
            Context = context;
            _logger = logger;
            _userService = userService;
            _client = client;
        }

        public Task SendMessageToUser(string message)
        {
            return _client.SendTextMessageAsync(_userService.CharmUser.Id, message, ParseMode.Html);
        }

        public async Task<List<Gist>> GetGistsFromContext(List<int> numbers)
        {
            if (numbers == null) throw new ArgumentNullException(nameof(numbers));

            DialogContext dialogInfo = GetUserDialogContext();
            if (dialogInfo.LastGistIds == null || dialogInfo.LastGistIds.Count == 0)
            {
                await SendMessageToUser("Контекст диалога пуст, попробуйте запросить список");
                return new List<Gist>();
            }

            var checkedNumbers = numbers.Select(n => n - 1).Where(n => n >= 0 && n < dialogInfo.LastGistIds.Count)
                .ToList();

            if (checkedNumbers.Count != numbers.Count)
            {
                await SendMessageToUser("Был введен неверный номер задачи, попробуйте обновить список");
            }

            var gistIds = checkedNumbers.Select(number => dialogInfo.LastGistIds[number]).ToList();
            return await Context.Gists.Where(g => gistIds.Contains(g.Id)).Include(g => g.Reminder).ToListAsync();
        }

        public DialogContext GetUserDialogContext()
        {
            return _userService.CharmUser.DialogContext ?? new DialogContext();
        }

        public async Task SetUserContext(DialogContext dialogContext)
        {
            if (dialogContext == null) throw new ArgumentNullException(nameof(dialogContext));

            var user = await Context.Users.FirstOrDefaultAsync(u => u.Id == _userService.CharmUser.Id);

            user.DialogContext = dialogContext;
            await Context.SaveChangesAsync();
        }

        public async Task<List<Gist>> SearchGists(long userId, GistSearchCriteria criteria)
        {
            IQueryable<Gist> query = Context.Gists;
            query = query.Where(g => g.UserId == userId);
            if (criteria.IsDone.HasValue)
            {
                query = query.Where(g => g.IsDone == criteria.IsDone);
            }

            if (criteria.Date.HasValue)
            {
                query = query.Where(g => g.Reminder == null ||
                                         g.Reminder!.Deadline >= criteria.Date.Value.DateTime.Date &&
                                         g.Reminder!.Deadline <= criteria.Date.Value.AddDays(1).DateTime.Date);
            }

            return await query.Include(g => g.Reminder).ToListAsync();
        }


        public async Task<List<Gist>> GetGists(long userId)
        {
            return await Context.Gists.Include(e => e!.Reminder).AsNoTracking().ToListAsync();
        }

        public async Task CreateGist(GistRequest request)
        {
            var gist = new Gist
            {
                Text = request.GistMessage,
                UserId = request.ChatId
            };
            Context.Gists.Add(gist);
            await Context.SaveChangesAsync();
        }

        public async Task CreateGistWithReminder(GistWithReminderRequest request)
        {
            var gist = new Gist
            {
                Text = request.GistMessage,
                UserId = request.ChatId
            };
            var reminder = new Reminder
            {
                Deadline = request.Deadline,
                Advance = request.Advance
            };
            gist.Reminder = reminder;

            Context.Gists.Add(gist);

            await Context.SaveChangesAsync();
        }

        public async Task CreateReminder(ReminderRequest request)
        {
            var reminder = new Reminder
            {
                GistId = request.GistId,
                Deadline = request.Deadline
            };
            Context.Reminders.Add(reminder);
            await Context.SaveChangesAsync();
        }
    }
}