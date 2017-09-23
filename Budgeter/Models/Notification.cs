using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Budgeter.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public DateTimeOffset Created { get; set; }
        public int? Account { get; set; }
        public decimal? Balance { get; set; }
        public int? Budget { get; set; }
        public decimal? Over { get; set; }

        [AllowHtml]
        public string NotifyReason { get; set; }

        //FK
        public string UserId { get; set; }

        //Nav
        public ApplicationUser User { get; set; }
    }
}