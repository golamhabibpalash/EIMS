using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using SMS.Entities;

namespace SMS_App.ViewModels.Accounts
{
    public class ExpenseTypesVM
    {
        public int Id { get; set; }
        public string ExpenseTypeName { get; set; }
        public string Description { get; set; }

        [Display(Name = "Edited By")]
        public string EditedBy { get; set; }

        [Display(Name = "Edited At")]
        public DateTime EditedAt { get; set; }
        public List<ExpenseType> ExpenseTypes { get; set; }
    }
}
