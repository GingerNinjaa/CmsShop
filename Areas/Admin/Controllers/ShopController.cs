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
                    CategoryDTO dto = new CategoryDTO();
                    dto.Name = catName;
                    dto.Slug = catName.Replace(" ", "-").ToLower();
                    dto.Sorting = 1000;

                    // ZAPIS DO BAZY 
                    db.Categories.Add(dto);
                    db.SaveChanges();

                    //pobieramy id
                    id = dto.Id.ToString();
                
            }
                return id;

        }
        //POST: Admin/Shop/ReorderCategories
        [HttpPost]
        public ActionResult ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {
                // inicjalizacja licznika 
                int count = 1;

                //deklaracja DTO
                CategoryDTO dto;

                //sortowanie kategori
                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);        //Przypisujemy do dto (pobrane z bazy kategorie o tym id)
                    dto.Sorting = count;                    // Zmieniamy dla tej kategori sorting

                    //Zapis na bazie danych
                    db.SaveChanges();

                    count++;
                }
            }
            return View();
        }
        //GET: Admin/Shop/DeleteCategory
        [HttpGet]
        public ActionResult DeleteCategory(int id)
        {
            using (Db db = new Db())
            {
                //Pobieramy Kategorie o podanym id 
                CategoryDTO dto = db.Categories.Find(id);

                // usuwamy kategorie
                db.Categories.Remove(dto);

                //zapis na bazie 
                db.SaveChanges();
            }

            // przekierowanie do widoku z categoriami
            return RedirectToAction("Categories");
        }
    }
}