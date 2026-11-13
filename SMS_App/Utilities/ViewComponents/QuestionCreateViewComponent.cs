using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SMS_App.ViewModels.QuestionBank;
using SMS.BLL.Contracts;
using System.Threading.Tasks;

namespace SMS_App.Utilities.ViewComponents
{
    public class QuestionCreateViewComponent : ViewComponent
    {
        private readonly IAcademicClassManager _academicClassManager;
        public QuestionCreateViewComponent(IAcademicClassManager academicClassManager)
        {
            _academicClassManager = academicClassManager;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            ViewData["AcademicClassId"] = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name");
            return View("Create",new QuestionVM());
        }
    }
}
