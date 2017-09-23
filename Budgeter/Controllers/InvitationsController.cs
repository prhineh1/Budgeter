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
using Microsoft.AspNet.Identity.EntityFramework;
using System.Threading.Tasks;

namespace Budgeter
{
    public class InvitationsController : Controller
    {
        private EmailHelper emailHelper = new EmailHelper();
        private ApplicationDbContext db = new ApplicationDbContext();
        
        [Authorize (Roles ="Admin, Head")]
        // GET: Invitations
        public ActionResult Index()
        {
            var invitations = db.Invitations.Include(i => i.Household);
            return View(invitations.ToList());
        }

        [Authorize(Roles = "Admin, Head")]
        // GET: Invitations/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Invitation invitation = db.Invitations.Find(id);
            if (invitation == null)
            {
                return HttpNotFound();
            }
            return View(invitation);
        }

        // GET: Invitations/Create
        public ActionResult Create()
        {
            ViewBag.HouseholdId = new SelectList(db.Households, "Id", "Name");
            return View();
        }

        // POST: Invitations/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Created,code,Email,accepted,expired,HouseholdId")] Invitation invitation)
        {
            var userId = User.Identity.GetUserId();
            var currentUser = db.Users.Find(userId);

            if (ModelState.IsValid)
            {
                invitation.HouseholdId = (int) currentUser.HouseHoldId;
                invitation.Created = DateTimeOffset.Now.DateTime;
                invitation.expiredDate = invitation.Created.AddDays(7);
                db.Invitations.Add(invitation);
                db.SaveChanges();

                var callbackUrl = Url.Action("Join", "Invitations", new {id = invitation.Id }, protocol: Request.Url.Scheme);
                var message = new EmailMessage()
                {
                    SourceName = "Sucre Lucre",
                    SourceId = userId,
                    DestinationEmail = invitation.Email,
                    Subject = String.Concat(currentUser.FullName, " has invited you to join ", db.Households.FirstOrDefault(i => i.Id == invitation.HouseholdId).Name),
                    Body = String.Concat("Enter this code ","<strong>", invitation.code, "</strong> ", "<a href=\"" + callbackUrl + "\">here</a>")
                };
                await emailHelper.SendNotificationEmailAsync(message);
                NotificationHelper.SendInviteEmail(invitation.Email, userId);

                return RedirectToAction("details", "HouseHolds" , new {id = currentUser.HouseHoldId });
            }

            ViewBag.HouseholdId = new SelectList(db.Households, "Id", "Name", invitation.HouseholdId);
            return View(invitation);
        }

        // GET: Invitations/Edit/5
        public ActionResult Join(int id)
        {
            Invitation invitation = db.Invitations.AsNoTracking().FirstOrDefault(i => i.Id == id);
            if (invitation == null)
            {
                TempData["warn"] = "deleted";
                return RedirectToAction("index", "home");
            }

            //check if the invitation has expired
            if (invitation.expiredDate < DateTimeOffset.Now)
            {
                invitation.expired = true;
                db.Entry(invitation).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("index", "home");
            }
            return View(invitation);
        }

        // POST: Invitations/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Join([Bind(Include = "Id,Created,code,Email,accepted,expired,HouseholdId")] Invitation invitation)
        {
            if (ModelState.IsValid)
            {
                var sentInvitation = db.Invitations.Find(invitation.Id);

                if (invitation.code != sentInvitation.code || invitation.Email != sentInvitation.Email)
                {
                    TempData["warn"] = "emailCode";
                    return View(invitation);
                }

                //check if user is in the database
                if (db.Users.Any(u => u.Email == invitation.Email))
                {
                    //check if user is in a household
                    var invitedUser = db.Users.FirstOrDefault(u => u.Email == invitation.Email).Id;
                    if (db.Households.SelectMany(h => h.Users).Any(u => u.Id == invitedUser))
                    {
                        TempData["warn"] = "user";
                        return View(invitation);
                    }
                    else
                    {
                        //add user to the household, send notification, delete invite
                        var currentUser = db.Users.FirstOrDefault(u => u.Email == sentInvitation.Email);
                        currentUser.HouseHoldId = sentInvitation.HouseholdId;
                        db.Entry(currentUser).State = EntityState.Modified;
                        db.Invitations.Remove(sentInvitation);
                        db.SaveChanges();
                        NotificationHelper.JoinedHousehold(currentUser.HouseHoldId, currentUser.Id);

                        if (User.Identity.IsAuthenticated)
                        {                           
                            return RedirectToAction("details", "Households", new {id = currentUser.HouseHoldId });
                        }
                        else
                        {
                            return RedirectToAction("Login", "Account");
                        }
                    }
                }
                else
                {
                    if (db.Invitations.Any(i => i.Email == sentInvitation.Email && i.accepted == true && i.HouseholdId != sentInvitation.HouseholdId))
                    {
                        TempData["warn"] = "accepted";
                        return View(invitation);
                    }
                    sentInvitation.accepted = true;
                    db.Entry(sentInvitation).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Register", "Account", new { email = sentInvitation.Email});
                };
            }
            ViewBag.HouseholdId = new SelectList(db.Households, "Id", "Name", invitation.HouseholdId);
            return View(invitation);
        }

        // GET: Invitations/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Invitation invitation = db.Invitations.Find(id);
            if (invitation == null)
            {
                return HttpNotFound();
            }
            return View(invitation);
        }

        // POST: Invitations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Invitation invitation = db.Invitations.Find(id);
            db.Invitations.Remove(invitation);
            db.SaveChanges();
            return RedirectToAction("Index");
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
