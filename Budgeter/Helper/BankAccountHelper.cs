using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Budgeter.Models;
using System.Runtime.CompilerServices;

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
                        account.Balance -= transaction.ReconciledAmount;
                    }
                    else if (!transaction.Expense && transaction.ReconciledAmount == 0)
                    {
                        account.Balance += transaction.Amount;
                    }
                    else if (!transaction.ReconciledExpense && transaction.ReconciledAmount > 0)
                    {
                        account.Balance += transaction.ReconciledAmount;
                    }
                }
                db.SaveChanges();

                if (account.Balance < 0)
                {
                    NotificationHelper.Overdraft(db.BankAccounts.Find(accountId).HouseholdId, accountId, account.Balance);
                }

                return  (account.Balance);
            }
            db.SaveChanges();
            return  account.Balance;
        }

        public static decimal BudgetItemAmount(int BudgetItemId)
        {
            var budgetItem = db.BudgetItems.AsNoTracking().FirstOrDefault(b => b.Id == BudgetItemId);

            decimal amount = 0;
            foreach (var transaction in budgetItem.Transactions.Where(t => t.Date.Month == DateTime.Now.Month).ToList())
            {
                if (transaction.ReconciledAmount == 0)
                {
                    amount += transaction.Amount;
                }
                else
                {
                    amount += transaction.ReconciledAmount;
                }
            }

            return amount;
        }

        public static decimal OverBudget(int budgetId)
        {
            decimal budgetAmount = db.Budgets.AsNoTracking().FirstOrDefault(b => b.Id == budgetId).Amount;
            decimal budgetExpenses = 0;
            
            if (db.BudgetItems.Where(b => b.BudgetId == budgetId).SelectMany(b => b.Transactions).ToList().Count > 0)
            {
                foreach (var item in db.BudgetItems.Where(b => b.BudgetId == budgetId).SelectMany(b => b.Transactions).ToList())
                {
                    if (item.ReconciledAmount == 0)
                    {
                        budgetExpenses += item.Amount;
                    }
                    else
                    {
                        budgetExpenses += item.ReconciledAmount;
                    }
                }

                if (budgetAmount < budgetExpenses)
                {
                    NotificationHelper.OverBudgetNotification(budgetId, budgetExpenses - budgetAmount);
                    return budgetExpenses - budgetAmount;
                }
            }
            NotificationHelper.OverBudgetNotification(budgetId, 0);
            return 0;
        }
    }
}