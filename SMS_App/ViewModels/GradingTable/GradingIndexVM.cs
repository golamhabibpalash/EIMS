using System.Reflection.Metadata.Ecma335;

namespace SMS_App.ViewModels.GradingTable
{
    public class GradingIndexVM
    {
        public int Id { get; set; }
        public string NumberRange { get; set; }
        public string LetterGrade { get; set; }
        public decimal GradePoint { get; set; }
        public string GradeComment { get; set; }
    }
}
