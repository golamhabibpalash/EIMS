using System;
using System.ComponentModel.DataAnnotations;

namespace SMS.Entities
{
    public class StudentActivateHist : CommonProps
    {
        public int StudentId { get; set; }
        public bool IsActive { get; set; }
        public DateTime ActionDateTime { get; set; } = DateTime.Now;
        [StringLength(5)]
        public string LastAction { get; set; }
    }
}
