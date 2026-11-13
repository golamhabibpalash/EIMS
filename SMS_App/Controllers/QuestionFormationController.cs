using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SMS_App.Utilities.MACIPServices;
using SMS.BLL.Contracts;
using SMS.Entities;
using System;
using System.Threading.Tasks;

namespace SMS_App.Controllers
{
    [Authorize]
    public class QuestionFormationController : Controller
    {
        private readonly IQuestionFormationManager _questionFormationManager;
        public QuestionFormationController(IQuestionFormationManager questionFormationManager)
        {
            _questionFormationManager = questionFormationManager;
        }

        // GET: QuestionFormationController
        [Authorize(Policy = "IndexQuestionFormationPolicy")]
        public async Task<ActionResult> Index()
        {
            var formats = await _questionFormationManager.GetAllAsync();
            return View(formats);
        }

        [HttpPost]
        [Authorize(Policy = "CreateQuestionFormationPolicy")]
        public async Task<JsonResult> CreatFormation(QuestionFormat model)
        {
            model.Id = 0;
            if (ModelState.IsValid)
            {
                try
                {
                    bool formationIsExist = false;
                    formationIsExist = await _questionFormationManager.GetQuestionFormatByNameAsync(model.Name);
                    if (formationIsExist)
                    {
                        return Json(new { qf = "", msg = "Formation name is already exist" });
                    }

                    formationIsExist = await _questionFormationManager.GetQuestionFormatByFormationAsync(model.QFormat);
                    if (formationIsExist)
                    {
                        return Json(new { qf = "", msg = "Same formation is already exist" });
                    }
                    model.MACAddress = MACService.GetMAC();
                    model.CreatedAt = DateTime.Now;
                    model.CreatedBy = HttpContext.Session.GetString("UserId");

                    bool isSaved = await _questionFormationManager.AddAsync(model);
                    if (isSaved)
                    {
                        return Json(new { qf = model, msg = "successfully saved" });
                    }
                    return Json(new { qf = "", msg = "Please provide all data correctly." }) ;
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return Json(new { qf = "", msg ="Please provide all data correctly."});
        }

        [Authorize(Policy = "EditQuestionFormationPolicy")]
        public async Task<JsonResult> EditFormation(QuestionFormat model)
        {
            bool isUpdated = await _questionFormationManager.UpdateAsync(model);
            if (isUpdated)
            {
                return Json(new {msg = "Updated Successfully"});
            }
            return Json(new { msg = "Fail to update" });
        }

        public async Task<JsonResult> GetById(int id)
        {
            var qf = await _questionFormationManager.GetByIdAsync(id);
            if (qf!=null)
            {
                return Json(qf);
            }
            return Json("");
        }
    }
}
