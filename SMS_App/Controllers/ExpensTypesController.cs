using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem;
using SMS_App.ViewModels.Accounts;
using SMS.DAL.Contracts;
using SMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS_App.Controllers
{
    [Authorize(Roles = "SuperAdmin, Admin")]
    public class ExpensTypesController : Controller
    {
        private readonly IExpenseTypeRepository _expenseTypeRepository;
        public ExpensTypesController(IExpenseTypeRepository expenseTypeRepository)
        {
            _expenseTypeRepository = expenseTypeRepository;
        }
        // GET: ExpensTypesController
        [Authorize(Policy = "IndexExpensTypesPolicy")]
        public async Task<ActionResult> Index()
        {
            GlobalUI.PageTitle = "Expense Type";
            ExpenseTypesVM expenseTypesVM = new ExpenseTypesVM();

            List<ExpenseType> objExpenseTypes = (List<ExpenseType>)await _expenseTypeRepository.GetAllAsync();
            expenseTypesVM.ExpenseTypes = objExpenseTypes;
            return View(expenseTypesVM);
        }

        // GET: ExpensTypesController/Details/5
        [Authorize(Policy = "DetailsExpensTypesPolicy")]
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ExpensTypesController/Create
        [Authorize(Policy = "CreateExpensTypesPolicy")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: ExpensTypesController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CreateExpensTypesPolicy")]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ExpensTypesController/Edit/5
        [Authorize(Policy = "EditExpensTypesPolicy")]
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ExpensTypesController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "EditExpensTypesPolicy")]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ExpensTypesController/Delete/5
        [Authorize(Policy = "DeleteExpensTypesPolicy")]
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ExpensTypesController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "DeleteExpensTypesPolicy")]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
