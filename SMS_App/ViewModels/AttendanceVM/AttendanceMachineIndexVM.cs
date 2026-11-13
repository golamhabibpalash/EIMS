using System;
using System.ComponentModel.DataAnnotations;

namespace SMS_App.ViewModels.AttendanceVM
{
    public class AttendanceMachineIndexVM
    {
        public int Id { get; set; }

        [Display(Name = "Card No")]
        public string CardNo { get; set; }
        public DateTime PunchDateTime { get; set; }
        public string Name { get; set; }
        public string GuardianPhone { get; set; }
        public string Designation { get; set; }
        public string Phone { get; set; }
        public string Attendance { get; set; }
        public string UserType { get; set; }

        [Display(Name = "Machine No")]
        public string MachineNo { get; set; }

        [Display(Name ="User Type")]
        public string UserInfo { get; set; }

        public string Remarks { get; set; }

    }
}
