using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMS.Entities
{
    public class Tran_MachineRawPunch
    {
        [Key]
        public int Tran_MachineRawPunchId { get; set; }
        [StringLength(20)]
        public string CardNo { get; set; }

        [Display(Name ="Punch Date Time")]
        public DateTime PunchDatetime { get; set; }
        public char P_Day { get; set; }
        public char ISManual { get; set; }

        [StringLength(50)]
        public string PayCode { get; set; }

        [StringLength(50)]
        public string MachineNo { get; set; }

        [Display(Name = "Verify Mode")]
        public int? VerifyMode { get; set; }

        [Display(Name = "Machine Serial No")]
        [StringLength(50)]
        public string MachineSerialNo { get; set; }

        [Display(Name = "Is Synced")]
        public bool IsSynced { get; set; } = false;
    }
}
