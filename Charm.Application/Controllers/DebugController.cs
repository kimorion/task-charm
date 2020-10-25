using Charm.Core.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Charm.Application.Controllers
{
    [ApiController]
    [Route("debug")]
    public class DebugController : ControllerBase
    {
        private readonly CharmInterpreter _interpreter;
        private readonly CharmDbContext _context;

        public DebugController(CharmInterpreter interpreter, CharmDbContext context)
        {
            _interpreter = interpreter;
            _context = context;
        }
    
        [HttpPost]
        public IActionResult TakeTextMessage(string message)
        {
            return Ok();
        }
    }
}