using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Charm.Core.Domain.Dto;
using Charm.Core.Infrastructure;
using Charm.Core.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Charm.Core.Domain.Services
{
    public class CharmManager
    {
        public readonly CharmDbContext Context;
        private readonly ILogger<CharmManager> _logger;

        public CharmManager(CharmDbContext context, ILogger<CharmManager> logger)
        {
            Context = context;
            _logger = logger;
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
            throw new NotImplementedException();
        }
    }
}