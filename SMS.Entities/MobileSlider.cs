using System;
using System.ComponentModel.DataAnnotations;

namespace SMS.Entities
{
    public class MobileSlider : CommonProps
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(300)]
        public string Subtitle { get; set; }

        [StringLength(500)]
        public string ImagePath { get; set; }

        [StringLength(500)]
        public string TargetUrl { get; set; }

        public bool IsPublished { get; set; }

        public DateTime? PublishedFrom { get; set; }

        public DateTime? PublishedTo { get; set; }

        public int DisplayOrder { get; set; }
    }
}
