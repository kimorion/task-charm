using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Charm.Core.Infrastructure.Repositories;
using Optional;

namespace Charm.Core.Domain
{
    public class CharmLibrarian
    {
        private readonly ReminderRepository _reminderRepository;
        private readonly TaskRepository _taskRepository;

        public CharmLibrarian(ReminderRepository reminderRepository, TaskRepository taskRepository)
        {
            _reminderRepository = reminderRepository;
            _taskRepository = taskRepository;
        }

        public async Task<Option<List<Task>, Exception>> GetTasks(Guid userId)
        {
            
        }

        public async Task SearchTasks()
        {
            throw new NotImplementedException();
        }

        public async Task<Option<string, Exception>> CreateTask(Guid userId, string gist)
        {
            throw new NotImplementedException();
        }

        public async Task CreateTaskWithReminder()
        {
            throw new NotImplementedException();
        }

        public async Task UpdateTask()
        {
            throw new NotImplementedException();
        }

        public async Task UpdateReminder()
        {
            throw new NotImplementedException();
        }

        public async Task DeleteTask()
        {
            throw new NotImplementedException();
        }

        public async Task DeleteReminder()
        {
            throw new NotImplementedException();
        }

        public async Task CreateSubtask()
        {
            throw new NotImplementedException();
        }

        public async Task AddReminderToTask()
        {
            throw new NotImplementedException();
        }
    }
}