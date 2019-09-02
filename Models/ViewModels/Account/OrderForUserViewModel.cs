using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CmsShop.Models.ViewModels.Account
{
    public class OrderForUserViewModel
    {
        public int OrderNumber { get; set; }
        public decimal Total { get; set; }
        public Dictionary<string, int> ProductsAnyQty { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}