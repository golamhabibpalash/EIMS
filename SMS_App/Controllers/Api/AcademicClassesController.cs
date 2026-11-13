using Microsoft.AspNetCore.Mvc;
using SMS.BLL.Contracts;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AcademicClassesController : ControllerBase
    {
        private readonly IAcademicClassManager _classManager;

        public AcademicClassesController(IAcademicClassManager classManager)
        {
            _classManager = classManager;
        }

        [HttpGet("GetAllActive")]
        public async Task<IActionResult> GetAllActive()
        {
            var classes = await _classManager.GetAllAsync();
            var activeClasses = classes.Where(s => s.Status == true)
                .Select(s => new { id = s.Id, name = s.Name })
                .ToList();
            return Ok(activeClasses);
        }
    }
}