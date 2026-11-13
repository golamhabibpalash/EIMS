using System;

namespace SMS_App.ViewModels.Students
{
    public class StudentActivateHistModel
    {
        public int StudentId { get; set; }
        public bool IsActive { get; set; }
        public DateTime ActionDateTime { get; set; }
    }
}
