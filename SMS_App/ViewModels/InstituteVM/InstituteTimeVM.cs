using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMS_App.ViewModels.InstituteVM
{
    public class InstituteTimeVM
    {
        [Display(Name=("Starting Time"))]
        public DateTime StartingTime { get; set; }

        [Display(Name = ("Closing Time"))]
        public DateTime ClosingTime { get; set; }

        [Display(Name = ("Late Start Time"))]
        public DateTime LateTimeStart { get ; set; }

        public IEquatable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            if (LateTimeStart>StartingTime)
            {
                results.Add(new ValidationResult("Late Start time must be higher than Starting Time"));
            }
            return (IEquatable<ValidationResult>)results;
        }
    }
}
