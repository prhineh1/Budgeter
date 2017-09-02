using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Budgeter.Models;
using Budgeter.Helper;
using Microsoft.AspNet.Identity;

namespace Budgeter.Controllers
{
    [Authorize]
    public class BankAccountsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: BankAccounts
        public ActionResult Index(int householdId)
        {
            var userId = User.Identity.GetUserId();
            var user = db.Users.Find(userId);
            if (user.HouseHoldId == null)
            {
                return RedirectToAction("index", "home");
            }
            if (user.HouseHoldId != householdId)
            {
                return RedirectToAction("details", "home", new { id = user.HouseHoldId });
            }
            var bankAccounts = db.BankAccounts.Where(b => b.HouseholdId == householdId);
            ViewBag.BudgetItemId = new SelectList(db.BudgetItems.Where(b => b.HouseholdId == householdId).ToList(), "Id", "Name", "Budget.Name", null, null);

            return View(bankAccounts.OrderBy(b => b.Id).ToList());
        }

        // POST: BankAccounts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Balance,HouseholdId")] BankAccount bankAccount)
        {
            if (ModelState.IsValid)
            {
                db.BankAccounts.Add(bankAccount);
                db.SaveChanges();
                NotificationHelper.NewBankAccount(bankAccount.HouseholdId, User.Identity.GetUserId(), bankAccount.Name);
                return RedirectToAction("index", "BankAccounts", new { id = bankAccount.HouseholdId});
            }

            return RedirectToAction("index", "BankAccounts", new { id = bankAccount.HouseholdId });
        }

        // POST: BankAccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id, string returnUrl)
        {
            BankAccount bankAccount = db.BankAccounts.Find(id);
            NotificationHelper.DeleteAccount(bankAccount.HouseholdId, bankAccount.Name);
            db.BankAccounts.Remove(bankAccount);
            db.SaveChanges();
            return Redirect(returnUrl);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
