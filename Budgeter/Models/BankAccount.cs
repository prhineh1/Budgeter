using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Budgeter.Models
{
    public class BankAccount
    {
        public int Id { get; set; }
        public string Name { get; set; }

        //[NotMapped]
        //private decimal balance = 0;

        public decimal Balance { get; set; }
        //{
        //    get
        //    {
        //        if (this.Transactions.Count() > 0)
        //        {
        //    balance = 0;
        //    foreach (var transaction in this.Transactions.ToList())
        //    {
        //        if (transaction.Expense && transaction.ReconciledAmount == 0)
        //        {
        //            balance -= transaction.Amount;
        //        }
        //        else if (transaction.Expense && transaction.ReconciledAmount > 0)
        //        {
        //            balance -= transaction.ReconciledAmount;
        //        }
        //        else if (!transaction.Expense && transaction.ReconciledAmount == 0)
        //        {
        //            balance += transaction.Amount;
        //        }
        //        else if (!transaction.Expense && transaction.ReconciledAmount > 0)
        //        {
        //            balance += transaction.ReconciledAmount;
        //        }
        //    }
        //    return balance;
        //}
        //        return balance;
        //}
        //    set {  }
        //}

        //FK
        public int HouseholdId { get; set; }

        //NAV
        public virtual Household Household { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }

        public BankAccount()
        {
            this.Transactions = new HashSet<Transaction>();
        }
    }
}