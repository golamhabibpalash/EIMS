using Microsoft.AspNetCore.Http;
using SMS.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SMS_App.ViewModels.QuestionBank
{
    public class QuestionCreateVM
    {
        [Required]
        public string Uddipok { get; set; } = string.Empty;
        public IFormFile Image { get; set; }

        [Display(Name ="Position")]
        public string ImagePosition { get; set; }
        public List<QuestionDetails> QuestionDetails { get; set; }        

        [Required, Display(Name = "Chapter")]
        public int ChapterId { get; set; }

        public int AcademicSubjectId { get; set; }
        public int AcademicClassId { get; set; }
    }
}
