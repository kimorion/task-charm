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
        private readonly CharmDbContext _context;
        private readonly ILogger<CharmManager> _logger;

        public CharmManager(CharmDbContext context, ILogger<CharmManager> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Gist>> GetGists(long userId)
        {
            return await _context.Gists.Include(e => e!.Reminder).AsNoTracking().ToListAsync();
        }

        public async Task CreateGist(GistRequest request)
        {
            var gist = new Gist
            {
                Text = request.GistMessage,
                UserId = request.ChatId
            };
            _context.Gists.Add(gist);
            await _context.SaveChangesAsync();
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

            _context.Gists.Add(gist);

            await _context.SaveChangesAsync();
        }

        // public async Task UpdateGist()
        // {
        //     throw new NotImplementedException();
        // }
        //
        // public async Task UpdateReminder()
        // {
        //     throw new NotImplementedException();
        // }
        //
        // public async Task DeleteGist()
        // {
        //     throw new NotImplementedException();
        // }
        //
        // public async Task DeleteReminder()
        // {
        //     throw new NotImplementedException();
        // }
        //
        // public async Task CreateSubGist()
        // {
        //     throw new NotImplementedException();
        // }
        //
        // public async Task AddReminderToTask()
        // {
        //     throw new NotImplementedException();
        // }
    }
}