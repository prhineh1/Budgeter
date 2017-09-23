using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Budgeter.Models
{
    public class Household
    {
        public int Id { get; set; }
        [AllowHtml]
        public string Name { get; set; }

        //Nav
        public virtual ICollection<ApplicationUser> Users { get; set; }
        public virtual ICollection<BankAccount> BankAccounts { get; set; }
        public virtual ICollection<Budget> Budgets { get; set; }
        public virtual ICollection<Invitation> Invitations { get; set; }
        public virtual ICollection<BudgetItem> BudgetItems { get; set; }

        public Household()
        {
            this.Users = new HashSet<ApplicationUser>();
            this.BankAccounts = new HashSet<BankAccount>();
            this.Budgets = new HashSet<Budget>();
            this.Invitations = new HashSet<Invitation>();
            this.BudgetItems = new HashSet<BudgetItem>();
        }
    }
}