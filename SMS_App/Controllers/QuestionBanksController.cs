using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SMS_App.Utilities.MACIPServices;
using SMS_App.ViewModels.QuestionBank;
using SMS.BLL.Contracts;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.Controllers
{
    [Authorize]
    public class QuestionBanksController : Controller
    {
        private readonly IAcademicClassManager _academicClassManager;
        private readonly IAcademicSubjectManager _academicSubjectManager;
        private readonly IQuestionManager _questionManager;
        private readonly IWebHostEnvironment _host;
        private readonly SMS_App.MultiTenancy.ITenantFileStorage _tenantFiles;

        public QuestionBanksController(IAcademicClassManager academicClassManager, IQuestionManager questionManager, IAcademicSubjectManager academicSubjectManager, IWebHostEnvironment host, SMS_App.MultiTenancy.ITenantFileStorage tenantFiles)
        {
        _tenantFiles = tenantFiles;
            _academicSubjectManager = academicSubjectManager;
            _academicClassManager = academicClassManager;
            _questionManager = questionManager;
            _host = host;
        }
        [Authorize(Policy = "IndexQuestionBanksPolicy")]
        public async Task<IActionResult> Index()
        {
            ViewData["AcademicClassId"] = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name");
            return View();
        }

        [Authorize(Policy = "AllQuestionQuestionBanksPolicy")]
        public async Task<IActionResult> AllQuestion() 
        {
            var questionBanks = await _questionManager.GetAllAsync();
            return View(questionBanks);
        }

        [HttpPost]
        [Authorize(Policy = "CreateQuestionBanksPolicy")]
        public async Task<JsonResult> CreateQuestion(QuestionVM model, IList<IFormFile> files, IFormFile Image)
        {
            Question nQuestion = new Question();            
            nQuestion.QuestionDetails = new List<QuestionDetails>();
            
            nQuestion.Uddipok = model.QCreateVM.Uddipok;
            if (model.QCreateVM.Image != null)
            {
                string questionImage = "";
                string timeDate = DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString().PadLeft(2,'0') + "_" + DateTime.Now.Day.ToString().PadLeft(2, '0') + "_" + DateTime.Now.ToString("hhmmss");
                string root = _tenantFiles.WebRootPath;
                if (string.IsNullOrEmpty(root))
                {
                    root = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                }
                string folder = "Images/Question";
                string fileExtension = Path.GetExtension(model.QCreateVM.Image.FileName);
                questionImage ="q_"+model.QCreateVM.AcademicClassId+"_"+model.QCreateVM.AcademicSubjectId+"_"+"_"+model.QCreateVM.ChapterId+"_"+ timeDate + fileExtension;
                string pathCombine = Path.Combine(root, folder, questionImage);
                using var stream = new FileStream(pathCombine, FileMode.Create);
                await model.QCreateVM.Image.CopyToAsync(stream);

                nQuestion.Image = questionImage;
            }
            nQuestion.ImagePosition = model.QCreateVM.ImagePosition;
            nQuestion.ChapterId = model.QCreateVM.ChapterId;
            if (model.QCreateVM.QuestionDetails.Count<=0)
            {
                return Json(new {msg="Questions did not found, try again."});
            }
            var existingSubject = await _academicSubjectManager.GetByIdAsync(model.QCreateVM.AcademicSubjectId);            
            List<int> qMarks = existingSubject.QuestionFormat.QFormat.Split(',').Select(int.Parse).ToList();
            int qSl = 0;
            foreach (var qd in model.QCreateVM.QuestionDetails)
            {
                qd.QuestionId = nQuestion.Id;
                qd.MACAddress = MACService.GetMAC();
                qd.CreatedBy = HttpContext.Session.GetString("UserId");
                qd.CreatedAt = DateTime.Now;
                qd.QMark = qMarks.ElementAt(qSl);
                nQuestion.QuestionDetails.Add(qd);
                qSl++;
            }
            nQuestion.CreatedAt = DateTime.Now;
            nQuestion.CreatedBy = HttpContext.Session.GetString("UserId");
            nQuestion.MACAddress = MACService.GetMAC();

            if (ModelState.IsValid)
            {
                try
                {
                   bool isSaved = await _questionManager.AddAsync(nQuestion);
                    if (isSaved)
                    {
                        return Json(new {newQuestion=nQuestion, msg = "New Question added successfully."});
                    }
                }
                catch (System.Exception)
                {
                    throw;
                }
            }
            return Json("");
        }

        [HttpGet]
        [Authorize(Policy = "EditQuestionBanksPolicy")]
        public async Task<IActionResult> Edit(int id)
        {
            var existingQuestion = await _questionManager.GetByIdAsync(id);

            QuestionEditVM questionEditVM = new QuestionEditVM();
            questionEditVM.Id = id; 
            List<QuestionDetails> questionDetails = new List<QuestionDetails>();
            foreach (var item in existingQuestion.QuestionDetails)
            {
                questionDetails.Add(item);
            }
            questionEditVM.QuestionDetails = questionDetails;
            questionEditVM.Uddipok = existingQuestion.Uddipok;
            questionEditVM.ChapterId = existingQuestion.ChapterId;
            if (existingQuestion.Image!=null)
            {
                questionEditVM.ImageUrl = existingQuestion.Image;
            }
            questionEditVM.ImagePosition = existingQuestion.ImagePosition;
            
            var aClass = await _academicClassManager.GetByIdAsync((int)existingQuestion.Chapter.AcademicSubject.AcademicClassId);
            questionEditVM.AcademicClassId = aClass.Id;
            var aSubject = await _academicSubjectManager.GetByIdAsync(existingQuestion.Chapter.AcademicSubjectId);
            questionEditVM.AcademicSubjectId = aSubject.Id;
            
            List<AcademicSubject> academicSubjects = new List<AcademicSubject>();
            academicSubjects.Add(aSubject);

            List<Chapter> chapters = new List<Chapter>();
            chapters.Add(existingQuestion.Chapter);

            ViewData["AcademicClassId"] = new SelectList(await _academicClassManager.GetAllAsync(), "Id", "Name",questionEditVM.AcademicClassId);
            ViewData["AcademicSubjectId"] = new SelectList(academicSubjects, "Id", "SubjectName", questionEditVM.AcademicSubjectId);
            ViewData["ChapterId"] = new SelectList(chapters, "Id", "ChapterName", questionEditVM.ChapterId);

            return View(questionEditVM);
        }


        [HttpPost]
        [Authorize(Policy = "EditQuestionBanksPolicy")]
        public async Task<IActionResult> Edit(QuestionEditVM model, IFormCollection form)
        {
            Question question = await _questionManager.GetByIdAsync(model.Id);
            question.ChapterId = model.ChapterId;
            question.MACAddress = MACService.GetMAC();
            question.EditedAt = DateTime.Now;
            question.EditedBy = HttpContext.Session.GetString("UserId");
            question.Uddipok = model.Uddipok;
            question.ImagePosition = model.ImagePosition;
            if (model.Image != null)
            {
                string questionImage = "";
                string timeDate = DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString().PadLeft(2, '0') + "_" + DateTime.Now.Day.ToString().PadLeft(2, '0') + "_" + DateTime.Now.ToString("hhmmss"); 
                string root = _tenantFiles.WebRootPath;
                if (string.IsNullOrEmpty(root))
                {
                    root = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                }
                string folder = "Images/Question";
                string fileExtension = Path.GetExtension(model.Image.FileName);
                questionImage = model.ImageUrl;
                if (string.IsNullOrEmpty(model.ImageUrl))
                {
                    questionImage = "q_" + model.AcademicClassId + "_" + model.AcademicSubjectId + "_" + "_" + model.ChapterId + "_" + timeDate + fileExtension;
                }
                string pathCombine = Path.Combine(root, folder, questionImage);
                using var stream = new FileStream(pathCombine, FileMode.Create);
                await model.Image.CopyToAsync(stream);

                question.Image = questionImage;
            }

            foreach (var item in model.QuestionDetails)
            {
                QuestionDetails qDetails = (from q in question.QuestionDetails
                                           where q.Id == item.Id
                                           select q).FirstOrDefault();

                qDetails.MACAddress = MACService.GetMAC();
                qDetails.EditedAt = DateTime.Now;
                qDetails.EditedBy = HttpContext.Session.GetString("UserId");
                qDetails.QuestionText = item.QuestionText;                
            }
            bool isUpdated = await _questionManager.UpdateAsync(question);
            if (isUpdated)
            {
                return RedirectToAction("AllQuestion");
            }
            return RedirectToAction("Edit", new {id=model.Id});
        }
    }
}
