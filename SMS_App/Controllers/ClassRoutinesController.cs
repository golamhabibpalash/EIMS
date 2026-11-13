using Microsoft.AspNetCore.Mvc;
using SMS_App.ViewModels;
using System;
using System.Collections.Generic;

namespace SMS_App.Controllers
{
    public class ClassRoutinesController : Controller
    {
        public IActionResult Index()
        {
            ClassRoutineVM classRoutine = new ClassRoutineVM();
            List<DaysVM> daysVMs = new List<DaysVM>();

            DaysVM daysVM1 = new DaysVM();
            daysVM1.DaysName = "Saturday";
            List<PeriodsVM> periodsVMs1 = new List<PeriodsVM>();
            PeriodsVM period1 = new PeriodsVM()
            {
                SL = 1,
                StartTime = new DateTime(0001, 01, 01, 09, 00, 0),
                EndTime = new DateTime(0001, 01, 01, 10, 00, 0),
            };
            periodsVMs1.Add(period1);
            daysVM1.Periods = periodsVMs1;
            daysVMs.Add(daysVM1);

            DaysVM daysVM2 = new DaysVM();
            daysVM2.DaysName = "Sunday";
            daysVMs.Add(daysVM2);

            DaysVM daysVM3 = new DaysVM();
            daysVM3.DaysName = "Monday";
            daysVMs.Add(daysVM3);

            DaysVM daysVM4 = new DaysVM();
            daysVM4.DaysName = "Tuesday";
            daysVMs.Add(daysVM4);

            DaysVM daysVM5 = new DaysVM();
            daysVM5.DaysName = "Wedsesnay";
            daysVMs.Add(daysVM5);
            classRoutine.Days = daysVMs;


            List<PeriodsVM> periodsVMs = new List<PeriodsVM>();



            return View(classRoutine);
        }
    }
}
