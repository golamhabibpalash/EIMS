using System.ComponentModel.DataAnnotations;
using System;
using SMS.Entities;
using System.Collections.Generic;

namespace SMS_App.ViewModels.ConfigureVM
{
    public class ParamBusConfigVM
    {
        public IList<ParamBusConfig> ParamBusConfigs { get; set; }
        public ParamBusConfig ParamBusConfig { get; set; }
    }
}
