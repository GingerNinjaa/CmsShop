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

        public ActionResult Category (string name)
        {
            //deklaracja listy produktów 
            List<ProductViewModel> productViewModelsList;
            using (Db db = new Db())
            {
                // pobranie id kategorii
                CategoryDTO categoryDTO = db.Categories.Where(x => x.Slug == name).FirstOrDefault();
                int catId = categoryDTO.Id;

                // inicjalizacja listy produktów
                productViewModelsList = db.Products
                    .ToArray()
                    .Where(x => x.CategoryId == catId)
                    .Select(x => new ProductViewModel(x))
                    .ToList();
                // TODO: DODAĆ IF-A W WYPADKU BRAKU PRODÓWKÓW Z DANEJ KATEGORI

                // pobieramy nazwe kategori
                var productCat = db.Products.Where(x => x.CategoryId == catId).FirstOrDefault();
                ViewBag.CategoryName = productCat.CategoryName;
            }

            //Zwracamy widok z listą produktów z danej kategorii
            return View(productViewModelsList);
        }
    }
}