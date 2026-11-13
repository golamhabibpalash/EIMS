using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMS_App.Utilities.ViewComponents
{
    public class ProfileOverlayViewComponent : ViewComponent
    {
        public  IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
