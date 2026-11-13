using System.Collections.Generic;

namespace SMS_App.ViewModels.MachineVM
{
    public class MachineEnrollmentIndexVM
    {
        public List<EmployeeMachineEnrollmentVM> Employees { get; set; } = new();
        public List<StudentMachineEnrollmentVM> Students { get; set; } = new();
    }

    public class EmployeeMachineEnrollmentVM
    {
        public int Id { get; set; }
        public string EmployeeName { get; set; }
        public string Designation { get; set; }
        public string Phone { get; set; }
        public string MachineUserId { get; set; }
        public bool Status { get; set; }
    }

    public class StudentMachineEnrollmentVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ClassName { get; set; }
        public int ClassRoll { get; set; }
        public string UniqueId { get; set; }
        public string MachineUserId { get; set; }
        public bool Status { get; set; }
    }
}