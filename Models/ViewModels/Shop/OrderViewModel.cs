﻿using CmsShop.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CmsShop.Models.ViewModels.Shop
{
    public class OrderViewModel
    {

        public OrderViewModel()
        {
                
        }
        public OrderViewModel(OrderDTO row)
        {
            OrderId = row.OrderId;
            UserId = row.UserId;
            CreatedAt = row.CreatedAt;
        }
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}