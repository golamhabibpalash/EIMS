using SMS.Entities;
using System.Collections.Generic;

namespace SMS_App.ViewModels.AcademicVM
{
    public class AcademicClassSubjectAllocationVM
    {
        public List<AcademicClassSubject> academicClassSubjects { get; set; }
        public int AcademicClassId { get; set; }
        public int AcademicSubjectId { get; set; }
    }
}
