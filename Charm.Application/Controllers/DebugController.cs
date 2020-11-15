using System;
using System.Threading.Tasks;
using Charm.Application.Dto;
using Charm.Core.Domain;
using Charm.Core.Infrastructure;
using Charm.Core.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Charm.Application.Controllers
{
    [ApiController]
    [Route("debug")]
    public class DebugController : ControllerBase
    {
        private readonly CharmInterpreter _interpreter;
        private readonly CharmDbContext _context;
        private readonly TaskRepository _taskRepository;
        private readonly CharmLibrarian _charmLibrarian;

        public DebugController(
            CharmInterpreter interpreter,
            CharmDbContext context,
            TaskRepository taskRepository,
            CharmLibrarian charmLibrarian)
        {
            _interpreter = interpreter;
            _context = context;
            _taskRepository = taskRepository;
            _charmLibrarian = charmLibrarian;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask(string message)
        {
            var responseOption = await _charmLibrarian.CreateTask(Guid.Empty, message);
            var response = responseOption.ValueOr(exception => exception.Message);

            return responseOption.HasValue ? Ok(response) : Problem(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTasksWithReminders()
        {
            var responseOption = await _charmLibrarian.GetTasks(Guid.Empty);
            var response = responseOption.ValueOr(exception => exception.Message);

            return responseOption.HasValue ? Ok(response) : Problem(response);
        }

        // [HttpPost]
        // public async Task<IActionResult> CreateTaskWithReminder([FromForm] TaskRequest request)
        // {
        // }

        [HttpPost]
        public async Task<IActionResult> TakeTextMessage(string message)
        {
            var responseOption = await _interpreter.TakeTextMessage(Guid.Empty, message);
            var response = responseOption.ValueOr(exception => exception.Message);

            return responseOption.HasValue ? Ok(response) : Problem(response);
        }
    }
}