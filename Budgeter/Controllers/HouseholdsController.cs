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
        public ActionResult Details(int? id)
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
            ViewBag.notifications = user.Notifications.ToList();
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
                if (household.Name.Trim().Length < 1)
                {
                    TempData["warn"] = "length";
                    return RedirectToAction("index", "home");
                }
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
        public ActionResult LeaveHousehold(string UserId, int householdId)
        {
            var user = db.Users.Find(UserId);

            if (RoleHelper.IsUserInRole(user.Id, "Head"))
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
        public ActionResult DeleteHousehold(string UserId, int householdId)
        {
            var household = db.Households.Find(householdId);
            foreach (var user in household.Users.ToList())
            {
                if (RoleHelper.IsUserInRole(user.Id, "Head"))
                {
                    RoleHelper.RemoveUserFromRole(user.Id, "Head");
                    db.Entry(user).State = EntityState.Modified;
                }
            }

            db.Households.Remove(household);
            db.SaveChanges();

            return RedirectToAction("index", "home");
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
