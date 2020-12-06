using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AutoMapper;
using Charm.Core.Domain;
using Charm.Core.Domain.Services;
using Charm.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Charm.Application.Controllers
{
    [ApiController]
    [Route("debug/interpreter/")]
    public class DebugInterpreterController : ControllerBase
    {
        private readonly CharmInterpreterService _interpreterService;
        private readonly CharmDbContext _context;
        private readonly CharmManager _charmManager;
        private readonly IMapper _mapper;

        public DebugInterpreterController(
            CharmInterpreterService interpreterService,
            CharmDbContext context,
            CharmManager charmManager, IMapper mapper)
        {
            _interpreterService = interpreterService;
            _context = context;
            _charmManager = charmManager;
            _mapper = mapper;
        }

        // [HttpPost]
        // public async Task<IActionResult> TakeMessage([Required] string message)
        // {
        //     throw new NotImplementedException();
        // }
    }
}