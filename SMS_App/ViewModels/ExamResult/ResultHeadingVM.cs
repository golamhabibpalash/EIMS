namespace SMS_App.ViewModels.ExamResult
{
    /// <summary>Heading shown above a processed result and repeated at the top of its exports.</summary>
    public class ResultHeadingVM
    {
        public string InstituteName { get; set; }
        public string InstituteAddress { get; set; }
        public string ExamName { get; set; }
        public string ClassName { get; set; }
        public string SessionName { get; set; }

        /// <summary>"Exam: X  |  Class: Y  |  Session: Z" (blank parts left out).</summary>
        public string DetailsLine
        {
            get
            {
                var parts = new System.Collections.Generic.List<string>();
                if (!string.IsNullOrWhiteSpace(ExamName)) parts.Add("Exam: " + ExamName);
                if (!string.IsNullOrWhiteSpace(ClassName)) parts.Add("Class: " + ClassName);
                if (!string.IsNullOrWhiteSpace(SessionName)) parts.Add("Session: " + SessionName);
                return string.Join("   |   ", parts);
            }
        }
    }
}
