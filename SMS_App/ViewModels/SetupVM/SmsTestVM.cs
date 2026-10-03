using System.ComponentModel.DataAnnotations;

namespace SMS_App.ViewModels.SetupVM
{
    public class SmsTestVM
    {
        [Display(Name = "Student")]
        public int? SampleStudentId { get; set; }

        public string SampleName { get; set; }

        [Display(Name = "Test mobile number")]
        public string TestNumber { get; set; }

        public string InstituteName { get; set; }

        public string PreviewCheckIn { get; set; }
        public string PreviewCheckOut { get; set; }
        public string PreviewAbsent { get; set; }

        public string Result { get; set; }
        public bool Sent { get; set; }
    }
}
