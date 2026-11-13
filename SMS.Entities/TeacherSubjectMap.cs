namespace SMS.Entities
{
    public class TeacherSubjectMap : CommonProps
    {
        public int AcademicEmployeeId { get; set; }
        public int AcademicClassSubjectId { get; set; }
        public Employee Employee { get; set; }
        public AcademicClassSubject ClassSubjects { get; set; }
    }
}
