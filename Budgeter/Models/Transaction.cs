using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Budgeter.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public bool Expense { get; set; }
        public bool ReconciledExpense { get; set; }
        public decimal ReconciledAmount { get; set; }

        //FK
        public int BankAccountId { get; set; }
        public string EnteredById { get; set; }
        public int? BudgetItemId { get; set; }

        //Nav
        public virtual BankAccount BankAccount { get; set; }
        public virtual ApplicationUser EnteredBy { get; set; }
        public virtual BudgetItem BudgetItem { get; set; }

        public Transaction()
        {
            this.EnteredById = HttpContext.Current.User.Identity.GetUserId();
        }
    }
}