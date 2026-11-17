using System;
using System.ComponentModel.DataAnnotations;

namespace SMS.Entities
{
    public class MobileNotice : CommonProps
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(2000)]
        public string Message { get; set; }

        [StringLength(500)]
        public string Url { get; set; }

        public bool IsPublished { get; set; }

        public DateTime? PublishedFrom { get; set; }

        public DateTime? PublishedTo { get; set; }

        public int DisplayOrder { get; set; }
    }
}
