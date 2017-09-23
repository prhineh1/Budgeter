using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Budgeter.Models.ViewModels
{
    public class _SharedPartialViewModel
    {
        public ApplicationUser User { get; set; }
        public List<Notification> Notifications { get; set; }
    }
}