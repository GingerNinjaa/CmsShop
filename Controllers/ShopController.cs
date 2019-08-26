using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
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
        // GET: /shop/Category/name
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

        //Get:/shop/product-szczegoly/name
        [ActionName("product-szczegoly")]
        public ActionResult ProductDetails(string name)
        {
            // deklaracja produktVm i productDTO

            ProductViewModel model;
            ProductDTO dto;

            //inicjalizacja product id 
            int id = 0;
            using (Db db = new Db())
            {
                //sprawdzamy czy produkt istnieje(ma taka samą nazwę)
                if (!db.Products.Any(x => x.Slug.Equals(name)))
                {
                    return RedirectToAction("Index", "Shop");
                }
                // inicjalizacja productDTO
                dto = db.Products.Where(x => x.Slug == name).FirstOrDefault();

                // pobranie id 
                id = dto.Id;


                //inicjalizacja modelu
                model = new ProductViewModel(dto);
            }
            // pobieramy galerie zdjec dla wybranego produktu 
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                .Select(fn => Path.GetFileName(fn));

            //zwalniamy widok z modelem
            return View("ProductDetails", model);
          
        }
    }
}