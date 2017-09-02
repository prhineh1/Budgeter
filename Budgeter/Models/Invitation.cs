using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Budgeter.Models
{
    public class Invitation
    {
        public int Id { get; set; }
        public DateTimeOffset Created { get; set; }
        public string code { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public bool accepted { get; set; }
        public bool expired { get; set; }
        public DateTimeOffset expiredDate { get; set; }

        //FK
        public int HouseholdId { get; set; }

        //NAV
        public Household Household { get; set; }

        public Invitation()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[8];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            this.code = new string(stringChars);
        }
    }
}