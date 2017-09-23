using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Budgeter.Models;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Microsoft.Ajax.Utilities;
using Budgeter.Helper;

namespace Budgeter.Controllers
{
    [Authorize]
    public class BudgetsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Budgets
        public async System.Threading.Tasks.Task<ActionResult> Index(int householdId)
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
            ViewBag.household = db.Households.Find(householdId);
            var budgets = db.Budgets.Include(b => b.Household).Include(b => b.BudgetItems).Where(b => b.HouseholdId == householdId);

            foreach(var budget in budgets.ToList())
            {
                if (BankAccountHelper.OverBudget(budget.Id) > 0)
                {
                    budget.over = true;
                }
                else
                {
                    budget.over = false;
                }
                db.Entry(budget).State = EntityState.Modified;
                db.SaveChanges();
            }
            db.SaveChanges();

            ViewBag.permission =  RoleHelper.IsUserInRole(userId, "Head") ||  RoleHelper.IsUserInRole(userId, "Admin");
            return View(budgets.ToList());
        }

        // GET: Budgets/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Budget budget = db.Budgets.Find(id);
            if (budget == null)
            {
                return HttpNotFound();
            }
            return View(budget);
        }

        // GET: Budgets/Create
        public ActionResult Create()
        {
            ViewBag.HouseholdId = new SelectList(db.Households, "Id", "Name");
            return View();
        }

        // POST: Budgets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Amount,HouseholdId")] Budget budget, string budgetItems, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (budgetItems != null)
                {
                    var budgetItemList = budgetItems.Split(",".ToCharArray());

                    foreach (var item in budgetItemList)
                    {
                        var BudgetItem = new BudgetItem()
                        {
                            Name = item,
                            HouseholdId = budget.HouseholdId
                        };
                        budget.BudgetItems.Add(BudgetItem);
                    }
                }

                db.Budgets.Add(budget);
                db.SaveChanges();
                return Redirect(returnUrl);
            }
            return Redirect(returnUrl);
        }

        // GET: Budgets/Edit/5
        [HttpGet]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Budget budget = db.Budgets.Find(id);
            var itemNameList = db.BudgetItems.Where(b => b.BudgetId == id).OrderBy(b => b.Id).Select(b => b.Name).ToList();
            var itemIdList = db.BudgetItems.Where(b => b.BudgetId == id).OrderBy(b => b.Id).Select(b => b.Id).ToList();
            if (budget == null)
            {
                return HttpNotFound();
            }
            var result = new
            {
                name = budget.Name,
                amount = budget.Amount,
                Id = budget.Id,
                budgetItemsNameEdit = itemNameList,
                budgetItemsIdEdit = itemIdList
            };
            return Content(JsonConvert.SerializeObject(result), "application/json");
        }

        // POST: Budgets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Edit([Bind(Include = "Id,Name,Amount,HouseholdId")] Budget budget)
        {
            var editBudget = db.Budgets.Find(budget.Id);
            if (!budget.Name.IsNullOrWhiteSpace())
            {
                editBudget.Name = budget.Name;
            }

            editBudget.Amount = budget.Amount;
            db.Entry(editBudget).State = EntityState.Modified;

            db.SaveChanges();

            var over = BankAccountHelper.OverBudget(editBudget.Id);
            if (over > 0)
            {
                editBudget.over = true;
            }
            else
            {
                editBudget.over = false;
            }
            db.Entry(editBudget).State = EntityState.Modified;
            db.SaveChanges();

            var result = new
            {
                name = editBudget.Name,
                amount = editBudget.Amount,
                over = over
            };
            return Content(JsonConvert.SerializeObject(result), "application/json");
        }

        // GET: Budgets/Delete/5
        [HttpGet]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Budget budget = db.Budgets.Include(b => b.BudgetItems).FirstOrDefault(b => b.Id == id);
            var budgetItemNames = db.BudgetItems.OrderBy(b => b.Id).Where(b => b.BudgetId == id).Select(b => b.Name).ToList();
            var budgetItemIds = db.BudgetItems.OrderBy(b => b.Id).Where(b => b.BudgetId == id).Select(b => b.Id).ToList();
            if (budget == null)
            {
                return HttpNotFound();
            }
            var result = new
            {
                name = budget.Name,
                Id = budget.Id,
                budgetItemNamesList = budgetItemNames,
                budgetItemIdsList = budgetItemIds
            };
            return Content(JsonConvert.SerializeObject(result), "application/json");
        }

        // POST: Budgets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int? id, string returnUrl)
        {
            if (id == null)
            {
                return Redirect(returnUrl);
            }

            Budget budget = db.Budgets.Find(id);
            foreach(var item in budget.BudgetItems.ToList())
            {
                foreach(var transaction in item.Transactions.ToList())
                {
                    db.Transactions.Remove(transaction);
                }
            }
            db.Budgets.Remove(budget);
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
