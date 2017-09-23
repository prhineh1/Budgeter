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
using System.Threading.Tasks;

namespace Budgeter.Controllers
{
    [Authorize]
    public class TransactionsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Transactions
        public  ActionResult Index(int accountId)
        {
            var householdId = db.BankAccounts.Find(accountId).HouseholdId;
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

            var transactions = db.Transactions.Include(t => t.BankAccount).Include(t => t.BudgetItem).Include(t => t.EnteredBy).Where(t => t.BankAccountId == accountId);
            ViewBag.account = db.BankAccounts.Find(accountId);
            ViewBag.accountBalance = BankAccountHelper.Balance(accountId);
            if (db.Transactions.Count() > 0)
            {
                ViewBag.maxAmount = db.Transactions.Where(t => t.BankAccountId == accountId).Select(t => t.Amount).Max();
                ViewBag.maxDate = db.Transactions.Where(t => t.BankAccountId == accountId).Select(t => t.Date).Max().Date;
            }
            ViewBag.BudgetItemId = new SelectList(db.BudgetItems.Where(b => b.HouseholdId == householdId).ToList(), "Id", "Name", "Budget.Name", null, null);
            ViewBag.permission =  RoleHelper.IsUserInRole(userId, "Head") ||  RoleHelper.IsUserInRole(userId, "Admin");
            return View(transactions.ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(List<int> filterAmount, List<DateTime> filterDate, int accountId)
        {
            var filterList = db.Transactions.AsNoTracking().Where(t => t.BankAccountId == accountId && t.Date >= filterDate.Min() && t.Date <= filterDate.Max()
                                                    && t.Amount >= filterAmount.Min() && t.Amount <= filterAmount.Max()).ToList();
            var transactions = db.Transactions.AsNoTracking().ToList();
            ViewBag.account = db.BankAccounts.Find(accountId);
            if (filterList.Count() > 0)
            {
                ViewBag.maxAmount = filterList.Where(t => t.BankAccountId == accountId).Select(t => t.Amount).Max();
                ViewBag.maxDate = filterList.Where(t => t.BankAccountId == accountId).Select(t => t.Date).Max().Date;
            }
            var householdId = db.BankAccounts.Find(accountId).HouseholdId;
            ViewBag.BudgetItemId = new SelectList(db.BudgetItems.Where(b => b.HouseholdId == householdId).ToList(), "Id", "Name", "Budget.Name", null, null);
            if (filterList.Count() < db.Transactions.Where(t => t.BankAccountId == accountId).Count())
            {
                TempData["refresh"] = "refresh";
            }
            var userId = User.Identity.GetUserId();
            ViewBag.accountBalance = BankAccountHelper.Balance(accountId);
            ViewBag.permission =  RoleHelper.IsUserInRole(userId, "Head") ||  RoleHelper.IsUserInRole(userId, "Admin");
            return View(filterList);

        }


        // POST: Transactions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Date,Amount,Expense,Reconciled,ReconciledAmount,BankAccountId,EnteredById,BudgetItemId")] Transaction transaction, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                transaction.Date = Convert.ToDateTime(transaction.Date);
                db.Transactions.Add(transaction);

                var userId = User.Identity.GetUserId();
                var householdId = (int)db.Users.FirstOrDefault(u => u.Id == userId).HouseHoldId;

                db.SaveChanges();
                if (transaction.BudgetItemId != null)
                {
                    BankAccountHelper.OverBudget(db.BudgetItems.FirstOrDefault(b => b.Id == transaction.BudgetItemId).BudgetId);
                }
                NotificationHelper.AddTransaction(userId, householdId, transaction.Id);
                if (returnUrl == null)
                {
                    return RedirectToAction("Index", "bankaccounts", new { householdId = db.BankAccounts.Find(transaction.BankAccountId).HouseholdId });
                }
                else
                {
                    return Redirect(returnUrl);
                }
            }

            if (returnUrl == null)
            {
                return RedirectToAction("Index", "bankaccounts", new { householdId = db.BankAccounts.Find(transaction.BankAccountId).HouseholdId });
            }
            else
            {
                return Redirect(returnUrl);
            }
        }

        // POST: Transactions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Date,Description,Amount,Expense,Reconciled,ReconciledAmount,BankAccountId,EnteredById,BudgetItemId")] Transaction transaction, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var oldTransaction = db.Transactions.AsNoTracking().FirstOrDefault(t => t.Id == transaction.Id);
                transaction.ReconciledExpense = transaction.Expense;
                transaction.Expense = oldTransaction.Expense;
                transaction.Date = Convert.ToDateTime(transaction.Date);

                db.Entry(transaction).State = EntityState.Modified;
                db.SaveChanges();
                if (transaction.BudgetItemId != null)
                {
                    BankAccountHelper.OverBudget(db.BudgetItems.FirstOrDefault(b => b.Id == transaction.BudgetItemId).BudgetId);
                }
                return Redirect(returnUrl);
            }

            return Redirect(returnUrl);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id, int accountId, string returnUrl)
        {
            Transaction transaction = db.Transactions.Find(id);
            db.Transactions.Remove(transaction);
            db.SaveChanges();
            BankAccountHelper.Balance(accountId);
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
