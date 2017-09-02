using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Budgeter.Models;

namespace Budgeter.Helper
{
    public static class BankAccountHelper
    {

        private static ApplicationDbContext db = new ApplicationDbContext();

        public static decimal Balance(int accountId)
        {
            var account = db.BankAccounts.AsNoTracking().FirstOrDefault(b => b.Id == accountId);
            account.Balance = 0;
            if (account.Transactions.Count() > 0)
                {
                    foreach (var transaction in account.Transactions.ToList())
                    {
                        if (transaction.Expense && transaction.ReconciledAmount == 0)
                        {
                            account.Balance -= transaction.Amount;
                        }
                        else if (transaction.ReconciledExpense && transaction.ReconciledAmount > 0)
                        {
                            account.Balance -=  transaction.ReconciledAmount;
                        }
                        else if (!transaction.Expense && transaction.ReconciledAmount == 0)
                        {
                            account.Balance += transaction.Amount;
                        }
                        else if (!transaction.ReconciledExpense && transaction.ReconciledAmount > 0)
                        {
                            account.Balance +=  transaction.ReconciledAmount;
                        }
                    }
                db.SaveChanges();

                if(account.Balance < 0)
                {
                    NotificationHelper.Overdraft(db.BankAccounts.Find(accountId).HouseholdId, accountId, account.Balance);
                }

                return account.Balance;
                }
            db.SaveChanges();
            return account.Balance;
        }
    }
}