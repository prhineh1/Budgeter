using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Budgeter.Models
{
    public class Budget
    {
        public int Id { get; set; }
        [AllowHtml]
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public bool over { get; set; }
        
        //FK
        public int HouseholdId { get; set; }

        //NAV
        public virtual Household Household { get; set; }
        public virtual ICollection<BudgetItem> BudgetItems { get; set; }

        public Budget()
        {
            this.BudgetItems = new HashSet<BudgetItem>();
        }
    }
}