using Microsoft.AspNetCore.Mvc;
using SMS.BLL.Contracts;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AcademicSessionsController : ControllerBase
    {
        private readonly IAcademicSessionManager _sessionManager;

        public AcademicSessionsController(IAcademicSessionManager sessionManager)
        {
            _sessionManager = sessionManager;
        }

        [HttpGet("GetAllActive")]
        public async Task<IActionResult> GetAllActive()
        {
            var sessions = await _sessionManager.GetAllAsync();
            var activeSessions = sessions.Where(s => s.Status == true)
                .Select(s => new { id = s.Id, name = s.Name })
                .ToList();
            return Ok(activeSessions);
        }
    }
}