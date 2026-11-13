namespace SMS.Entities
{
    public class ClassRoutine : CommonProps
    {
        public int EmployeeId { get; set; }
        public int AcademicSectionId { get; set; }
        public int AcademicSubjectId { get; set; }
        public int DaysId { get; set; }
        public int ClassPeriodId { get; set; }
        public int ClassRoomId { get; set; }
        public int AcademicClassId { get; set; }
        public AcademicClass AcademicClass { get; set; }
        public Employee Employee { get; set; }
        public AcademicSubject AcademicSubject { get; set; }
        public Days Days { get; set; }
        public ClassPeriods ClassPeriods { get; set; }
        public ClassRoom ClassRoom { get; set; }
    }
}
