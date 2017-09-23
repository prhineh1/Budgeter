using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Budgeter.Models;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;

namespace Budgeter.Controllers
{
    public class BudgetItemsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: BudgetItems
        public ActionResult Index()
        {
            var budgetItems = db.BudgetItems.Include(b => b.Budget).Include(b => b.Household);
            return View(budgetItems.ToList());
        }

        // GET: BudgetItems/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BudgetItem budgetItem = db.BudgetItems.Find(id);
            if (budgetItem == null)
            {
                return HttpNotFound();
            }
            return View(budgetItem);
        }

        // GET: BudgetItems/Create
        public ActionResult Create()
        {
            ViewBag.BudgetId = new SelectList(db.Budgets, "Id", "Name");
            ViewBag.HouseholdId = new SelectList(db.Households, "Id", "Name");
            return View();
        }

        // POST: BudgetItems/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int BudgetId, string BudgetItems)
        {
            var BudgetItemsList = BudgetItems.Split(",".ToCharArray()).ToList();
            var householdid = db.Budgets.Find(BudgetId).HouseholdId;
            foreach (var name in BudgetItemsList)
            {
                var budgetItem = new BudgetItem()
                {
                    Name = name,
                    HouseholdId = householdid,
                    BudgetId = BudgetId
                };
                db.BudgetItems.Add(budgetItem);
            }

            db.SaveChanges();
            return RedirectToAction("Index","Budgets", new { householdId = householdid});

        }

        // GET: BudgetItems/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BudgetItem budgetItem = db.BudgetItems.Find(id);
            if (budgetItem == null)
            {
                return HttpNotFound();
            }

            return Content(budgetItem.Name, "string");
        }

        // POST: BudgetItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Edit([Bind(Include = "Id,Name,BudgetId,HouseholdId")] BudgetItem budgetItem)
        {
            var editBudgetItem = db.BudgetItems.Find(budgetItem.Id);
            if (!budgetItem.Name.IsNullOrWhiteSpace())
            {
                editBudgetItem.Name = budgetItem.Name;
            }
            db.Entry(editBudgetItem).State = EntityState.Modified;
            db.SaveChanges();

            var result = new
            {
                name = editBudgetItem.Name,
                Id = editBudgetItem.Id
            };

            return Content(JsonConvert.SerializeObject(result), "application/json");
            }

        // GET: BudgetItems/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BudgetItem budgetItem = db.BudgetItems.Find(id);
            if (budgetItem == null)
            {
                return HttpNotFound();
            }
            return View(budgetItem);
        }

        // POST: BudgetItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(List<int> Ids, string returnUrl)
        {
            if (Ids == null)
            {
                return Redirect(returnUrl);
            }
            foreach(var id in Ids)
            {
                BudgetItem budgetItem = db.BudgetItems.Find(id);
                foreach(var transaction in budgetItem.Transactions.ToList())
                {
                    db.Transactions.Remove(transaction);
                }
                db.BudgetItems.Remove(budgetItem);
            }
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
