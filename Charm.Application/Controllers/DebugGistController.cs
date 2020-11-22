using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Charm.Application.Dto;
using Charm.Core.Domain;
using Charm.Core.Domain.Services;
using Charm.Core.Infrastructure;
using Charm.Core.Infrastructure.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Charm.Application.Controllers
{
    [ApiController]
    [Route("debug-gist/")]
    public class DebugGistController : ControllerBase
    {
        private readonly CharmInterpreter _interpreter;
        private readonly CharmDbContext _context;
        private readonly CharmManager _charmManager;
        private readonly IMapper _mapper;
        private readonly long _debugUserId = 1;

        public DebugGistController(
            CharmInterpreter interpreter,
            CharmDbContext context,
            CharmManager charmManager, IMapper mapper)
        {
            _interpreter = interpreter;
            _context = context;
            _charmManager = charmManager;
            _mapper = mapper;

            // Create debug user if not exists
            var debugUser = context.Users.SingleOrDefault(e => e!.Id.Equals(_debugUserId));
            if (debugUser is null)
            {
                User user = new User
                {
                    Id = _debugUserId,
                    Name = "Debug"
                };
                context.Users.Add(user);
                context.SaveChanges();
                context.ChangeTracker.Clear();
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateGist([FromForm] GistRequest request)
        {
            await _charmManager.CreateGist(new Core.Domain.Dto.GistRequest
            {
                GistMessage = request.Message ?? throw new NullReferenceException(),
                ChatId = _debugUserId
            });
            return Ok();
        }

        [HttpPost("reminder")]
        public async Task<IActionResult> CreateGistWithReminder([FromForm] GistWithReminderRequest request)
        {
            await _charmManager.CreateGistWithReminder(new Core.Domain.Dto.GistWithReminderRequest
            {
                GistMessage = request.Message ?? throw new NullReferenceException(),
                ChatId = _debugUserId,
                Deadline = request.Deadline ?? throw new NullReferenceException(),
                Advance = request.Advance
            });
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetGists()
        {
            var result = await _charmManager.GetGists(_debugUserId);
            result = result.Select(e =>
            {
                var r = e.Reminder;
                if (r is not null)
                {
                    r.Gist = null!;
                }

                return e;
            }).ToList();
            return Ok(result);
        }

        // [HttpDelete]
        // public async Task<IActionResult> DeleteGist()
        // {
        //     throw new NotImplementedException();
        // }
        //
        // [HttpPut]
        // public async Task<IActionResult> UpdateGist()
        // {
        //     throw new NotImplementedException();
        // }
    }
}