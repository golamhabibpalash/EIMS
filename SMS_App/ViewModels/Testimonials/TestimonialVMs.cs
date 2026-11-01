using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SMS_App.ViewModels.Testimonials
{
    public class TestimonialCreateVM
    {
        [Display(Name = "Student (optional)")]
        public int? StudentId { get; set; }

        [Required(ErrorMessage = "Display name is required."), StringLength(150), Display(Name = "Display Name")]
        public string StudentName { get; set; }

        [StringLength(150), Display(Name = "Designation / Class / Batch")]
        public string Designation { get; set; }

        [Required(ErrorMessage = "Testimonial text is required."), Display(Name = "Testimonial")]
        public string Content { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Active")]
        public bool Status { get; set; } = true;

        [Display(Name = "Published")]
        public bool IsPublished { get; set; }

        public IEnumerable<SelectListItem> StudentList { get; set; } = new List<SelectListItem>();
    }

    public class TestimonialEditVM : TestimonialCreateVM
    {
        public int Id { get; set; }

        /// <summary>Existing photo filename (kept unless a new one is uploaded).</summary>
        public string Photo { get; set; }
    }
}
