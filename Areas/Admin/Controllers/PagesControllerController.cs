using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShop.Areas.Admin.Controllers
{
    public class PagesControllerController : Controller
    {
        // GET: Admin/PagesController
        public ActionResult Index()
        {
            return View();
        }
    }
}