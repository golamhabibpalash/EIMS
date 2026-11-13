using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SMS_App.Controllers
{
    public class ExpensesController : Controller
    {
        // GET: ExpensesController
        public ActionResult Index()
        {
            return View();
        }

        // GET: ExpensesController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ExpensesController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ExpensesController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        // GET: ExpensesController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ExpensesController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        // GET: ExpensesController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ExpensesController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
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
