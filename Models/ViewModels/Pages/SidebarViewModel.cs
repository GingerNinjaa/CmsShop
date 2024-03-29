﻿using CmsShop.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShop.Models.ViewModels.Pages
{
    public class SidebarViewModel
    {
    public SidebarViewModel()
        {
        }

        public SidebarViewModel(SidebarDTO row)
        {
            Id = row.Id;
            Body = row.Body;
        }
        public int Id { get; set; }
        [AllowHtml]
        public string Body { get; set; }
    }
}