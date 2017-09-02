using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Budgeter.Models;

namespace Budgeter.Helper
{
    public static class NotificationHelper
    {
        private static ApplicationDbContext db = new ApplicationDbContext();

        public static void JoinedHousehold(int? householdId, string userId)
        {
            var Household = db.Households.Find(householdId);
            var addedUser = db.Users.Find(userId);
            foreach (var user in Household.Users.SkipWhile(u => u == addedUser).ToList())
            {
                var notification = new Notification()
                {
                    Created = DateTimeOffset.Now,
                    NotifyReason = "<strong>" + addedUser.FullName + "</strong>" + " has joined the household!",
                    UserId = user.Id
                };
                db.Notifications.Add(notification);
            }

            db.SaveChanges();
        }

        public static void JoinedHouseholdNewUser(int? householdId, string userId)
        {
            var Household = db.Households.Find(householdId);
            var addedUser = db.Users.Find(userId);
            foreach (var user in Household.Users.SkipWhile(u => u == addedUser).ToList())
            {
                var notification = new Notification()
                {
                    Created = DateTimeOffset.Now,
                    NotifyReason = "<strong>" + addedUser.FullName + "</strong>" + " has joined the household!",
                    UserId = user.Id
                };
                db.Notifications.Add(notification);
            }

            db.SaveChanges();
        }

        public static void SendInviteEmail (string email, string userId)
        {
            var notification = new Notification()
            {
                Created = DateTimeOffset.Now,
                NotifyReason = "An invite Email has been sent to " + "<strong>" + email + "</strong>",
                UserId = userId
            };
            db.Notifications.Add(notification);
            db.SaveChanges();
        }

        public static void LeftHousehold (int HouseholdId, string userId)
        {
            var household = db.Households.Find(HouseholdId);

            foreach(var user in household.Users.ToList())
            {
                var notification = new Notification()
                {
                    Created = DateTimeOffset.Now,
                    NotifyReason = "<strong>" + db.Users.Find(userId).FullName + "</strong>" + " has left the household",
                    UserId = user.Id
                };
                db.Notifications.Add(notification);
                db.SaveChanges();
            }
        }

        public static void NewBankAccount(int householdId, string userId, string bankName)
        {
            var household = db.Households.Find(householdId);

            foreach(var user in household.Users.ToList())
            {
                var notification = new Notification()
                {
                    Created = DateTimeOffset.Now,
                    NotifyReason = "<strong>" +  db.Users.Find(userId).FirstName + "</strong> has added a new account: " + "<strong>" + bankName + "</strong>", 
                    UserId = user.Id
                };
                db.Notifications.Add(notification);
                db.SaveChanges();
            }
        }

        public static void Overdraft(int householdId, int accountId, decimal balance)
        {
            var household = db.Households.Find(householdId);
            var account = db.BankAccounts.Find(accountId);
            var oldNotification = db.Notifications.OrderByDescending(n => n.Created).FirstOrDefault(n => n.Account == accountId);
            decimal oldBalance;

            if(oldNotification == null)
            {
                oldBalance = 0;
            }
            else
            {
                oldBalance = (decimal) oldNotification.Balance;
            }

            if (balance < oldBalance)
            {
                foreach (var user in household.Users.ToList())
                {
                    var notification = new Notification()
                    {
                        Created = DateTimeOffset.Now,
                        NotifyReason = "<strong>" + account.Name + "</strong>" + " has been overdraft by $<strong> " + Math.Abs(balance) + "</strong>",
                        UserId = user.Id,
                        Account = accountId,
                        Balance = balance
                    };
                    db.Notifications.Add(notification);
                    db.SaveChanges();
                }
            }
        }

        public static void DeleteAccount(int householdId, string bankName)
        {
            var household = db.Households.Find(householdId);

            foreach (var user in household.Users.ToList())
            {
                var notification = new Notification()
                {
                    Created = DateTimeOffset.Now,
                    NotifyReason = "<strong>" + bankName + "</strong>" + " has been deleted",
                    UserId = user.Id
                };
                db.Notifications.Add(notification);
                db.SaveChanges();
            }
        }

        public static void AddTransaction(string userId, int householdId, int transactionId)
        {
            var household = db.Households.Find(householdId);
            var transaction = db.Transactions.Find(transactionId);
            foreach (var user in household.Users.ToList())
            {
                var notification = new Notification()
                {
                    Created = DateTimeOffset.Now,
                    UserId = user.Id
                };

                if (transaction.Expense)
                {
                    notification.NotifyReason = "Transaction by<strong> " + db.Users.Find(userId).FirstName + "</strong> in<strong> " + db.Transactions.Find(transactionId).BudgetItem.Name + "</strong> for <strong> " + db.Transactions.Find(transactionId).Amount + "</strong> in<strong> " + transaction.BankAccount.Name + "</strong>";
                }
                else
                {
                    notification.NotifyReason = "An income of<strong> " + transaction.Amount + "</strong> has been added to<strong> " + transaction.BankAccount.Name + "</strong>";
                }
                db.Notifications.Add(notification);
                db.SaveChanges();
            }
        }
    }
}