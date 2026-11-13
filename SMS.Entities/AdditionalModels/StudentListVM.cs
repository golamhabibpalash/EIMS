namespace SMS.Entities.AdditionalModels
{
    public class StudentListVM
    {
        public int Id { get; set; }
        public int ClassRoll { get; set; }
        public string Photo { get; set; }
        public string StudentName { get; set; }
        public string NameBangla { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public string PhoneNo { get; set; }
        public string SessionName { get; set; }
        public string Gender { get; set; }
        public bool Status { get; set; }
        public int? ClassSerial { get; set; }
        public bool IsResidential { get; set; }
        public string UniqueId { get; set; }
        public string GuardianPhone { get; set; }
        public int AcademicSessionId { get; set; }
    }
}
