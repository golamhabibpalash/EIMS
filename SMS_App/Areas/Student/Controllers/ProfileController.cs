using Microsoft.AspNetCore.Mvc;

namespace SMS_App.Areas.Student.Controllers
{
    public class ProfileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
