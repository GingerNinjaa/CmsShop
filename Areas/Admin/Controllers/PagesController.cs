using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShop.Areas.Admin.Controllers
{
    public class PagesController : Controller
    {
        // GET: Admin/PagesController
        public ActionResult Index()
        {

            // Deklaracja listy PageViewModel
            List<PageViewModel> pagesList;

            //Inicjalizacja listy
            using (Db db = new Db())
            {
                pagesList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageViewModel(x)).ToList();
            }
          
            // Zwracamy strony do widoku 
            return View(pagesList);
        }
    }
}