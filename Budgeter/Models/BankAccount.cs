using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Budgeter.Models
{
    public class BankAccount
    {
        public int Id { get; set; }

        [AllowHtml]
        public string Name { get; set; }

        public decimal Balance { get; set; }


        //FK
        public int HouseholdId { get; set; }

        //NAV
        public virtual Household Household { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }

        public BankAccount()
        {
            this.Transactions = new HashSet<Transaction>();
            Balance = 0M;
        }
    }
}