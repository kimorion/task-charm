using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Charm.Core.Domain.Dto;
using Charm.Core.Infrastructure;
using Charm.Core.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Charm.Core.Domain.Services
{
    public class CharmManager
    {
        private readonly CharmDbContext _context;

        public CharmManager(CharmDbContext context)
        {
            _context = context;
        }

        public async Task<List<Gist>> GetGists(Guid userId)
        {
            return await _context.Gists.Include(e => e!.Reminder).AsNoTracking().ToListAsync();
        }

        public async Task SearchGists()
        {
            throw new NotImplementedException();
        }

        public async Task CreateGist(GistRequest request)
        {
            var gist = new Gist
            {
                Text = request.GistMessage,
                UserId = request.UserId
            };
            _context.Gists.Add(gist);
            await _context.SaveChangesAsync();
        }

        public async Task CreateGistWithReminder(GistWithReminderRequest request)
        {
            var gist = new Gist
            {
                Text = request.GistMessage,
                UserId = request.UserId
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

        public async Task UpdateGist()
        {
            throw new NotImplementedException();
        }

        public async Task UpdateReminder()
        {
            throw new NotImplementedException();
        }

        public async Task DeleteGist()
        {
            throw new NotImplementedException();
        }

        public async Task DeleteReminder()
        {
            throw new NotImplementedException();
        }

        public async Task CreateSubGist()
        {
            throw new NotImplementedException();
        }

        public async Task AddReminderToTask()
        {
            throw new NotImplementedException();
        }
    }
}