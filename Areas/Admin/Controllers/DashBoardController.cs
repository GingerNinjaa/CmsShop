﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShop.Areas.Admin.Controllers
{
    [Authorize(Roles ="Admin")]
    public class DashboardController : Controller
    {
        // GET: Admin/DashBoard
        public ActionResult Index()
        {
            return View();
        }
    }
}