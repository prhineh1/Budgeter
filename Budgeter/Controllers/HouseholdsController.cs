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
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Budgeter.Controllers
{
    public class HouseholdsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Households
        public ActionResult Index()
        {
            //Household household = db.Households.Find(id);
            return View();
        }

        // GET: Households/Details/5
        [Authorize]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var household = db.Households.FirstOrDefault(h => h.Id == id);
            if (household == null)
            {
                return HttpNotFound();
            }
            var userId = User.Identity.GetUserId();
            var user = db.Users.Find(userId);
            if (user.HouseHoldId == null)
            {
                return RedirectToAction("index", "home");
            }
            if (user.HouseHoldId != household.Id)
            {
                return RedirectToAction("details", "home", new { id = user.HouseHoldId });
            }
            ViewBag.Budgets = db.Budgets.Where(b => b.HouseholdId == user.HouseHoldId).ToList();
            ViewBag.permission =  RoleHelper.IsUserInRole(userId, "Head") ||  RoleHelper.IsUserInRole(userId, "Admin");
            return View(household);
        }

        // POST: Households/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name")] Household household)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId();
                var CurrentUser = db.Users.Find(userId);

                if (db.Households.SelectMany(h => h.Users).Any(u => u.Id == userId))
                {
                    TempData["warn"] = "user";
                    return RedirectToAction("index", "Home");
                }

                RoleHelper.AddUserToRole(userId, "Head");
                household.Users.Add(CurrentUser);

                var subCategories = new Dictionary<string, List<string>>()
            {
                {"Utilities", new List<string>() {"Gas", "Electric", "Water" }},
                {"Food and Groceries", new List<string>() { "Eating Out", "Kitchen Supplies", "Alcohol"}},
                {"Housing", new List<string>() { "Maintenance and Repairs", "Insurance", "Taxes"}},
                {"Entertainment", new List<string>() {"Movies", "Books", "Music" }}
            };

                int counter = 1;
                foreach (var category in subCategories.Keys)
                {
                    var budget = new Budget()
                    {
                        Name = category,
                        Amount = 500,
                        Id = counter
                    };

                   household.Budgets.Add(budget);

                    foreach (var subCategory in subCategories[category])
                    {
                        var budgetItem = new BudgetItem()
                        {
                            Name = subCategory,
                            BudgetId = counter
                        };
                        household.BudgetItems.Add(budgetItem);
                    }
                    counter++;
                }

                db.Households.Add(household);
                db.SaveChanges();

                return RedirectToAction("details", new { id = household.Id});
            }

            return View(household);
        }

        // POST: Households/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Household household = db.Households.Find(id);
            db.Households.Remove(household);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteNotification(List<int> ids, string returnUrl)
        {
            if (ids == null)
            {
                return Redirect(returnUrl);
            }
            var userId = User.Identity.GetUserId();
            var currentUser = db.Users.Find(userId);

            foreach(int id in ids)
            {
                //Remove the notifications from the user
                var notification = db.Notifications.Find(id);
                currentUser.Notifications.Remove(notification);
                db.SaveChanges();
            }
            return Redirect(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LeaveHousehold(string UserId, int householdId)
        {
            var user = db.Users.Find(UserId);

            if ( RoleHelper.IsUserInRole(user.Id, "Head"))
            {
                RoleHelper.RemoveUserFromRole(user.Id, "Head");
            }

            user.HouseHoldId = null;
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();
            NotificationHelper.LeftHousehold(householdId, UserId);

            return RedirectToAction("index", "home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteHousehold(string UserId, int householdId)
        {
            var household = db.Households.Find(householdId);
            foreach (var user in household.Users.ToList())
            {
                if ( RoleHelper.IsUserInRole(user.Id, "Head"))
                {
                    RoleHelper.RemoveUserFromRole(user.Id, "Head");
                    db.Entry(user).State = EntityState.Modified;
                }
            }

            db.Households.Remove(household);
            db.SaveChanges();

            return RedirectToAction("index", "home");
        }

        [HttpGet]
        public ActionResult BarGraphInfo(int? BudgetId, int HouseholdId)
        {
            int month = DateTime.Today.Month;
            decimal transactions = 0;
            decimal income = 0;
            decimal budget = 0;
            List<BudgetItem> budgetItems;
            List<decimal> transactionByBudgetItemList = new List<decimal>();

            if (BudgetId == 0 || BudgetId == null)
            {
                foreach (var item in db.BudgetItems.AsNoTracking().Where(b => b.HouseholdId == HouseholdId).SelectMany(b => b.Transactions).Where(t => t.Date.Month == month).ToList())
                {
                    if (item.ReconciledAmount == 0)
                    {
                        transactions += item.Amount;
                    }
                    else if ( item.ReconciledAmount > 0)
                    {
                        transactions += item.ReconciledAmount;
                    }
                }
                
                foreach (var item in db.Budgets.AsNoTracking().Where(b => b.HouseholdId == HouseholdId).ToList())
                {
                    budget += item.Amount;
                }

                var result1 = new
                {
                    transactions = transactions,
                    budget = budget,
                    title = "Overall Budget and Expenses"
                };

                return Content(JsonConvert.SerializeObject(result1), "application/json");
            }
            else if (BudgetId == -1)
            {
                foreach (var item in db.BankAccounts.AsNoTracking().Where(b => b.HouseholdId == HouseholdId).SelectMany(b => b.Transactions).Where(t => t.Date.Month == month && t.BudgetItemId == null).ToList())
                {
                    if (item.ReconciledAmount == 0)
                    {
                        income += item.Amount;
                    }
                    else if (item.ReconciledAmount > 0)
                    {
                        income += item.ReconciledAmount;
                    }
                }
                foreach (var item in db.BudgetItems.AsNoTracking().Where(b => b.HouseholdId == HouseholdId).SelectMany(b => b.Transactions).Where(t => t.Date.Month == month).ToList())
                {
                    if (item.ReconciledAmount == 0)
                    {
                        transactions += item.Amount;
                    }
                    else if (item.ReconciledAmount > 0)
                    {
                        transactions += item.ReconciledAmount;
                    }
                }

                var result2 = new
                {
                    transactions = transactions,
                    income = income,
                    title = "Income and Expenses"
                };

                return Content(JsonConvert.SerializeObject(result2), "application/json");
            }
            else
            {
                budgetItems = db.BudgetItems.AsNoTracking().Where(b => b.HouseholdId == HouseholdId && b.BudgetId == BudgetId).OrderBy(b => b.Id).ToList();
                foreach (var item in budgetItems)
                {
                    decimal TransactionByBudgetItem = 0;
                    foreach (var transaction in item.Transactions.Where(t => t.Date.Month == month).ToList())
                    {
                        if (transaction.ReconciledAmount == 0)
                        {
                            TransactionByBudgetItem += transaction.Amount;
                        }
                        else if (transaction.ReconciledAmount > 0)
                        {
                            TransactionByBudgetItem += transaction.ReconciledAmount;
                        }
                    }
                    transactionByBudgetItemList.Add(TransactionByBudgetItem);
                }

                budget += db.Budgets.Find(BudgetId).Amount;

                var result3 = new
                {
                    transactionsList = transactionByBudgetItemList,
                    budgetItemList = budgetItems.Select(b => b.Name),
                    title = db.Budgets.AsNoTracking().FirstOrDefault(b => b.Id == BudgetId).Name + " Budget and Expenses",
                    budgetAmount = budget
                };

                return Content(JsonConvert.SerializeObject(result3), "application/json");
            }
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
