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

namespace Budgeter.Controllers
{
    [Authorize]
    public class BudgetsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Budgets
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
            ViewBag.household = db.Households.Find(householdId);
            var budgets = db.Budgets.Include(b => b.Household).Include(b => b.BudgetItems).Where(b => b.HouseholdId == householdId);
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
        public ActionResult Edit(int? id)
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
            ViewBag.HouseholdId = new SelectList(db.Households, "Id", "Name", budget.HouseholdId);
            return View(budget);
        }

        // POST: Budgets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Amount,HouseholdId")] Budget budget)
        {
            if (ModelState.IsValid)
            {
                db.Entry(budget).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.HouseholdId = new SelectList(db.Households, "Id", "Name", budget.HouseholdId);
            return View(budget);
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
        public ActionResult DeleteConfirmed(int id, string returnUrl)
        {
            Budget budget = db.Budgets.Find(id);
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
