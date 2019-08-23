using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Shop;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
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
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }
            }

            // sprawdzenie czy nazwa produktu jest unikalna 
            using (Db db = new Db())
            {
                if (db.Products.Any(x => x.Name == model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
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

            // Utworzenie potrzebnej struktury katalogów\
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() );
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs" );   // miniaturka
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery" );
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs" );

            if (!Directory.Exists(pathString1))
                Directory.CreateDirectory(pathString1);
            if (!Directory.Exists(pathString2))
                Directory.CreateDirectory(pathString2);
            if (!Directory.Exists(pathString3))
                Directory.CreateDirectory(pathString3);
            if (!Directory.Exists(pathString4))
                Directory.CreateDirectory(pathString4);
            if (!Directory.Exists(pathString5))
                Directory.CreateDirectory(pathString5);


            if (file != null && file.ContentLength > 0)
            {
                // sprawdzenie rozszeżenia pliku czy mamy doczynienia z obrazkiem
                string ext = file.ContentType.ToLower();
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/PNG" &&
                    ext != "image/png")
                {

                    using (Db db = new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "Obraz nie został przesłany - nieprawidłowe rozszerzenia obrazu.");
                        return View(model);
                    }
                }
                // inicjalizacja nazwy obrazka
                string imageName = file.FileName;

                // zapis nazwy obrazka do bazy 
                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;

                    // zapisywanie na bazie 
                    db.SaveChanges();
                }
                var path = string.Format("{0}\\{1}", pathString2, imageName);
                var path2 = string.Format("{0}\\{1}", pathString3, imageName);

                // zapisujemy originalny obrazek
                file.SaveAs(path);

                // Zapisujemy miniaturke
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }

            #endregion

            return RedirectToAction("AddProduct");
        }
        //GET: Admin/Shop/Products
        [HttpGet]
        public ActionResult Products(int? page, int? catId)
        {
            //Deklaracja listy Produktów 
            List<ProductViewModel> listOfProductVM;

            //Ustawiamy numer strony
            var pageNumber = page ?? 1;

            using (Db db = new Db())
            {
                //inicjalizacja listy produktów 
                listOfProductVM = db.Products.ToArray()
                                    .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                                    .Select(x => new ProductViewModel(x))
                                    .ToList();
                // lista kategori do dropDownList
                ViewBag.Categories = new SelectList (db.Categories.ToList(), "Id", "Name");

                // ustawiamy wybrana kategorie 
                ViewBag.SelectedCat = catId.ToString();
            }

            // ustawienie stronnicowania 
            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 3);
            ViewBag.OnePageOfProducts = onePageOfProducts;

            //zwracamy widok z lista produktów
            return View(listOfProductVM);

            
        }
    }
}