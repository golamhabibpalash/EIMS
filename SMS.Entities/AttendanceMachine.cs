using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMS.Entities
{
    public enum MachineBrand
    {
        ZKTeco = 1,
        RealTime = 2,
        Suprema = 3,
        Matrix = 4,
        Other = 99
    }

    public class AttendanceMachine : CommonProps
    {
        [Display(Name = "Machine Name")]
        [Required, StringLength(100)]
        public string Name { get; set; }

        [Display(Name = "IP Address")]
        [Required, StringLength(50)]
        public string IPAddress { get; set; }

        [Display(Name = "Port")]
        public int Port { get; set; } = 4370;

        [Display(Name = "Serial Number")]
        [StringLength(50)]
        public string SerialNumber { get; set; }

        [Display(Name = "Model")]
        [StringLength(50)]
        public string Model { get; set; }

        [Display(Name = "Brand")]
        public MachineBrand Brand { get; set; } = MachineBrand.ZKTeco;

        [Display(Name = "Location")]
        [StringLength(200)]
        public string Location { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Use Push Mode")]
        public bool UsePushMode { get; set; } = true;

        [Display(Name = "Last Sync At")]
        public DateTime? LastSyncAt { get; set; }

        [Display(Name = "Last Error")]
        [StringLength(500)]
        public string LastError { get; set; }

        [Display(Name = "Push Endpoint")]
        [StringLength(200)]
        public string PushEndpoint { get; set; } = "/api/attendance/push";

        [Display(Name = "Username")]
        [StringLength(50)]
        public string Username { get; set; } = "admin";

        [Display(Name = "Password")]
        [StringLength(50)]
        public string Password { get; set; } = "admin";
    }
}