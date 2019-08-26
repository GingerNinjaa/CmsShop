using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace CmsShop.Controllers
{
    public class ShopController : Controller
    {
        // GET: Shop
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Pages");
        }

        public ActionResult CategoryMenuPartial()
        {
            //Deklarujemy CategoryVM list
            List<CategoryViewModel> categoryViewModelList;

            // inicjalizacja listy 
            using (Db db = new Db())
            {
                categoryViewModelList = db.Categories
                    .ToArray()
                    .OrderBy(x => x.Sorting)
                    .Select(x => new CategoryViewModel(x))
                    .ToList();
            }

            // zwracamy partial z listą 

            return PartialView(categoryViewModelList);
        }
    }
}