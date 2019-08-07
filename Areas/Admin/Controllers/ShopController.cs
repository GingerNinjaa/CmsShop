using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShop.Areas.Admin.Controllers
{
    public class ShopController : Controller
    {
        // GET: Admin/Shop/Categories
        public ActionResult Categories()
        {
            //Delklaracja listy kategorii do wyświetlenia 
            List<CategoryViewModel> categoryViewModel;

            using (Db db = new Db())
            {
                categoryViewModel = db.Categories
                                      .ToArray()
                                      .OrderBy(x => x.Sorting)
                                      .Select(x => new CategoryViewModel(x)).ToList();
            }


            return View(categoryViewModel);
        }
        //POST: Admin/Shop/AddNewCategory
        [HttpPost]
        public string  AddNewCategory(string catName)
        {
            // Deklaracja id

            string id;
            using (Db db = new Db())
            {
                // Sprawdzanie unikalności
                if (db.Categories.Any(x => x.Name == catName))   //Sprawdzenie czy dana kategoria istnieje w bazie danych 
                {
                    return "tytułzajety";
                }
                
                    // inicjalizacja DTO
                    CategoryDTO DTO = new CategoryDTO();
                    DTO.Name = catName;
                    DTO.Slug = catName.Replace(" ", "=").ToLower();
                    DTO.Sorting = 1000;

                    // ZAPIS DO BAZY 
                    db.Categories.Add(DTO);
                    db.SaveChanges();

                    //pobieramy id
                    id = DTO.Id.ToString();
                
            }
                return id;
        }
    }
}