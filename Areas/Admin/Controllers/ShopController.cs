﻿using CmsShop.Areas.Admin.Models.ViewModels.Shop;
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
    [Authorize(Roles = "Admin")]
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
                    ModelState.AddModelError("", "Ta nazwa produktu jest zajęta!");
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
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");   // miniaturka
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

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
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                // ustawiamy wybrana kategorie 
                ViewBag.SelectedCat = catId.ToString();
            }

            // ustawienie stronnicowania 
            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 3);
            ViewBag.OnePageOfProducts = onePageOfProducts;

            //zwracamy widok z lista produktów
            return View(listOfProductVM);

        }

        //GET: Admin/Shop/EditProduct/id
        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            //deklaracja productViewModel
            ProductViewModel model;

            using (Db db = new Db())
            {
                // pobieramy produkt do edycji 
                ProductDTO dto = db.Products.Find(id);

                //Sprawdzamy czy ten produkt istnieje
                if (dto == null)
                {
                    return Content("Ten produkt nie istnieje");
                }

                // inicjalizacja modelu 
                model = new ProductViewModel(dto);

                //lista kategori
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                // ustawiamy zdjecia
                model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));
            }
            return View(model);
        }

        // POST: Admin/Shop/EditProduct
        [HttpPost]
        public ActionResult EditProduct(ProductViewModel model, HttpPostedFileBase file)
        {
            // pobranie id produktu
            int id = model.Id;

            // pobranie kategorii dla listy rozwijanej
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }

            // ustawiamy zdjecia
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                                            .Select(fn => Path.GetFileName(fn));

            // sprawdzamy model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // sprawdzenie unikalnosci nazwy produktu
            using (Db db = new Db())
            {
                if (db.Products.Where(x => x.Id != id).Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("", "Ta nazwa produktu jest zajęta");
                    return View(model);
                }
            }

            // Edycja produktu i zapis na bazie
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Description = model.Description;
                dto.Price = model.Price;
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;

                CategoryDTO catDto = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDto.Name;

                db.SaveChanges();
            }

            // ustawienie TempData
            TempData["SM"] = "Edytowałeś produkt";

            #region Image Upload

            // sprawdzamy czy jest plik
            if (file != null && file.ContentLength > 0)
            {
                // sprawdzamy rozszerzenie pliku czy to jest obrazek
                string ext = file.ContentType.ToLower();

                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "Obraz nie został przesłany - nieprawidłowe rozszerzenie obrazu.");
                        return View(model);
                    }
                }

                // Utworzenie potrzebnej struktury katalogów
                var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

                var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");

                // usuwamy stare pliki z katalogow
                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);

                foreach (var file2 in di1.GetFiles())
                    file2.Delete();

                foreach (var file3 in di2.GetFiles())
                    file3.Delete();

                // Zapis nazwy obrazka na bazie
                string imageName = file.FileName;

                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }

                // Zapis nowych plików
                var path = string.Format("{0}\\{1}", pathString1, imageName);
                var path2 = string.Format("{0}\\{1}", pathString2, imageName);

                file.SaveAs(path);

                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }

            #endregion


            return RedirectToAction("EditProduct");
        }

        //GET: Admin/Shop/DeleteProduct/id
        [HttpGet]
        public ActionResult DeleteProduct(int id)
        {
            // usuniecie produktow z bazy
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);
                db.SaveChanges();
            }

            //Usuniecie produktu z wszystkimi plikami 
            var orginalDirector = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
            var pathString = Path.Combine(orginalDirector.ToString(), "Products\\" + id.ToString());

            if (Directory.Exists(pathString))
            {
                Directory.Delete(pathString, true);
            }

            return RedirectToAction("Products");
        }

        // POST: Admin/Shop/SaveGalleryImages/id
        [HttpPost]
        public ActionResult SaveGalleryImages(int id)
        {
            // petla po obrazkach
            foreach (string fileName in Request.Files)
            {
                // inicjalizacja
                HttpPostedFileBase file = Request.Files[fileName];

                //Sprawdzenie czy mamy plik i czy nie jest pusty
                if (file != null && file.ContentLength > 0)
                {
                    // ustawiamy ścieżki do katalogow
                    var oryginalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

                    string pathString1 = Path.Combine(oryginalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
                    string pathString2 = Path.Combine(oryginalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

                    var path = string.Format("{0}\\{1}", pathString1, file.FileName);
                    var path2 = string.Format("{0}\\{1}", pathString2, file.FileName);

                    //Zapis obrazków i miniaturek
                    file.SaveAs(path);

                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(200, 200);
                    img.Save(path2);
                }
            }

            return View();
        }

        // POST: Admin/Shop/DeleteImage
        [HttpPost]
        public void DeleteImage(int id, string imageName)
        {
            string fullPath1 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/" + imageName);
            string fullPath2 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/Thumbs/" + imageName);

            if (System.IO.File.Exists(fullPath1))
                System.IO.File.Delete(fullPath1);

            if (System.IO.File.Exists(fullPath2))
                System.IO.File.Delete(fullPath2);
        }

        //GET: Admin/Shop/Orders
        [HttpGet]
        public ActionResult Orders()
        {

            //inicjalizacja OrderForAdminViewModel
            List<OrdersForAdminViewModel> ordersForAdminViewModel = new List<OrdersForAdminViewModel>();
            using (Db db = new Db())
            {
                //pobieramy zamównienia 
                List<OrderViewModel> orders = db.Orders.ToArray().Select(x => new OrderViewModel(x)).ToList();

                foreach (var order in orders)
                {
                    //inicjalizacja slownika produktów 
                    Dictionary<string, int> productAndQty = new Dictionary<string, int>();

                    decimal total = 0m;

                    // inicjalizacja ordersDetailDTO
                    List<OrderDetailsDTO> orderDetailsList = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    //pobieramy użytkownika
                    UserDTO user = db.Users.Where(x => x.Id == order.UserId).FirstOrDefault();
                    string username = user.UserName;

                    foreach (var orderDetails in orderDetailsList)
                    {
                        //pobieramy produkt 
                        ProductDTO product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();

                        //pobieramy cene produktu
                        decimal price = product.Price;

                        //poberamy nazwe produktu
                        string productname = product.Name;

                        //dodac produkt do słownika 
                        productAndQty.Add(productname, orderDetails.Quantity);

                        //ustawiamy podsumowanie total
                        total += orderDetails.Quantity * price;
                    }
                    ordersForAdminViewModel.Add(new OrdersForAdminViewModel()
                    {
                        OrderNumber = order.OrderId,
                        UserName = username,
                        Total = total,
                        ProductsAnyQty = productAndQty,
                        CreatedAt = order.CreatedAt
                    });
                }
            }
            return View(ordersForAdminViewModel);
        }

    }
}