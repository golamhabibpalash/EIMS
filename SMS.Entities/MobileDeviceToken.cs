using System;
using System.ComponentModel.DataAnnotations;

namespace SMS.Entities
{
    public class MobileDeviceToken : CommonProps
    {
        [Required, StringLength(450)]
        public string UserId { get; set; }

        [Required, StringLength(450)]
        public string Token { get; set; }

        [StringLength(20)]
        public string Platform { get; set; } = "android";

        public DateTime LastSeenAt { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
