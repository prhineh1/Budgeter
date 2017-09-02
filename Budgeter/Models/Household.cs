using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Budgeter.Models
{
    public class Household
    {
        public int Id { get; set; }
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

            //var subCategories = new Dictionary<string, List<string>>()
            //{
            //    {"Utilities", new List<string>() {"Gas", "Electric", "Water" }},
            //    {"Food and Groceries", new List<string>() { "Eating Out", "Kitchen Supplies", "Alcohol"}},
            //    {"Housing", new List<string>() { "Maintenance and Repairs", "Insurance", "Taxes"}},
            //    {"Entertainment", new List<string>() {"Movies", "Books", "Music" }}
            //};

            //int counter = 1;
            //foreach (var category in subCategories.Keys)
            //{
            //    var budget = new Budget()
            //    {
            //        Name = category,
            //        Amount = 500,
            //        Id = counter
            //    };

            //    this.Budgets.Add(budget);

            //    foreach (var subCategory in subCategories[category])
            //    {
            //        var budgetItem = new BudgetItem()
            //        {
            //            Name = subCategory,
            //            Category = category,
            //            BudgetId = counter
            //        };
            //        this.BudgetItems.Add(budgetItem);
            //    }
            //    counter++;
            //}
        }
    }
}