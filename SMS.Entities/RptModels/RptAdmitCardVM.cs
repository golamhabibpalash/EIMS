using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SMS.Entities.RptModels
{
    public class RptAdmitCardVM
    {
        public int StudentId { get; set; }
        public int ClassRoll { get; set; }
        public string StudentName { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public string SessionName { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public int? AcademicSectionId { get; set; }
        public int MonthId { get; set; }
        public int? SubjectCode { get; set; }
        public string SubjectName { get; set; }
        public int AcademicClassId { get; set; }
        public string ExamTypeName { get; set; }
        public string ExamGroupName { get; set; }
        public string InstituteName { get; set; }
        public string EIIN { get; set; }
        public int GenderId { get; set; }
        public string Gender { get; set; }
        public string Religion { get; set; }
        public bool StudentStauts { get; set;}
    }
}
