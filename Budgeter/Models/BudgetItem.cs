using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Budgeter.Models
{
    public class BudgetItem
    {
        public int Id { get; set; }
        [AllowHtml]
        public string Name { get; set; }

        //FK
        public int BudgetId { get; set; }
        public int? HouseholdId { get; set; }

        //Nav
        public virtual Budget Budget { get; set; }
        public virtual Household Household { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }

        public BudgetItem()
        {
            this.Transactions = new HashSet<Transaction>();
        }
    }
}