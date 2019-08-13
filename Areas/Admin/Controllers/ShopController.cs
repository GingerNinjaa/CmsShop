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
        public string AddNewCategory(string catName)
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

        //POST: Admin/Shop/RenameCategory
        [HttpPost]
        public string RenameCategory(string newCatName, int id)
        {
            using (Db db = new Db())
            {
                //Sprawdzenie czy kategoria jest unikalna
                if (db.Categories.Any(x => x.Name == newCatName))
                {
                    return "tytułzajety";
                }

                // pobieramy kategorie
                CategoryDTO dto = db.Categories.Find(id);

                //Edycja kategorii
                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();

                // zapis na bazie 
                db.SaveChanges();
            }

            return "OK";
        }

        //GET: Admin/Shop/AddProduct
        [HttpGet]
        public ActionResult AddProduct()
        {
            // Inicjalizacja model
            ProductViewModel model = new ProductViewModel();

            //pobieramy liste kategorii
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

            }

            return View(model);
        }
        //POST: Admin/Shop/AddProduct
        [HttpPost]
        public ActionResult AddProduct(ProductViewModel model, HttpPostedFileBase file)
        {
            // Sprawdzamy model State
            if (!ModelState.IsValid)
            {
                using (Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "id", "Name");
                    return View(model);
                }
            }

            // sprawdzenie czy nazwa produktu jest unikalna 
            using (Db db = new Db())
            {
                if (db.Products.Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("","Ta nazwa produktu jest zajęta!");
                    return View(model);
                }
            }

            //Deklaracja product id 
            int id;

            // dodawanie produktu i zapis na bazie 
            using (Db db = new Db())
            {
                ProductDTO product = new ProductDTO();
                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;

                CategoryDTO catDto = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                product.CategoryName = catDto.Name;

                db.Products.Add(product);
                db.SaveChanges();

                // pobranie id dodanego produktu
                id = product.Id;
            }

            // ustawiamy komunikat 
            TempData["SM"] = "Dodałeś produkt";
            #region Upload Image



            #endregion

            return View();
        }
    }
}