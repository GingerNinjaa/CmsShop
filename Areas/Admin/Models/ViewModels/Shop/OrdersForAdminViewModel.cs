using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CmsShop.Areas.Admin.Models.ViewModels.Shop
{
    public class OrdersForAdminViewModel
    {
        public int OrderNumber { get; set; }
        public string UserName { get; set; }
        public decimal Total { get; set; }
        public Dictionary<string, int> ProductsAnyQty { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}