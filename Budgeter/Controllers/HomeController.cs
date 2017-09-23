using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Budgeter.Models;
using Budgeter.Models.ViewModels;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Budgeter.Controllers
{
    [RequireHttps]
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId();
                var user = db.Users.Find(userId);
                ViewBag.householdId = user.HouseHoldId;
            }
            else
            {
                ViewBag.householdId = null;
            }
            ViewBag.permission = User.Identity.IsAuthenticated;
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpGet]
        public ActionResult NotificationCount(string userId)
        {
            var count = db.Notifications.Where(n => n.UserId == userId).Count().ToString();

            return Content(count, "string");
        }

        public ActionResult _SharedPartial()
        {
            var userId = User.Identity.GetUserId();
            var user = db.Users.Find(userId);
            var viewModel = new _SharedPartialViewModel()
            {
                User = db.Users.Find(userId),
                Notifications = db.Notifications.Where(n => n.UserId == userId).ToList()
            };
            return View(viewModel);
        }
    }
}