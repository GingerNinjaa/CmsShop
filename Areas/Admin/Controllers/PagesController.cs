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

        // GET: Admin/Pages/AddPages
        [HttpGet]
        public ActionResult addPage()
        {
            return View();
        }

        // GET: Admin/Pages/AddPages
        [HttpPost]
        public ActionResult addPage(PageViewModel model)
        {
            // Sprawdzanie model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                string slug;

                //Inicjalizacja PageDTO
                PageDTO dto = new PageDTO();

                // Gdy niemamy adresu strny to przypisujemy tytuł
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ","-").ToLower();
                }
                else
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();
                }

                //Zapobiegamy dodawaniu takiej samej strony
                if (db.Pages.Any(x => x.Title == model.Title) || db.Pages.Any(x => x.Slug == model.Slug))
                {
                    ModelState.AddModelError("", "Ten tytuł lub adres strony  juz istnieje");
                    return View(model);
                }
                dto.Title = model.Title;
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 1000;

                // Zapis DTO
                db.Pages.Add(dto);
                db.SaveChanges();

            }

            TempData["SM"] = "Dodałeś nową stronę";

            return RedirectToAction("AddPage");
        }
        
        // GET: Admin/Pages/EditPage
        [HttpGet]
        public ActionResult EditPage(int id)
        {
            // Deklaracja PageViewModel
            PageViewModel model;

            using (Db db = new Db())
            {
                // Pobieramy strone z bazy o podanym id
                PageDTO dto = db.Pages.Find(id);

                // Sprawdzamy czy taka strona istnieje
                if (dto == null)
                {
                    return Content("Strona nie istnieje");
                }
                model = new PageViewModel(dto);


            }

            return View(model);
        }

        // POST: Admin/Pages/EditPage
        public ActionResult EditPage(PageViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                // Pobranie Id strony którą chcemy edytować

                int id = model.Id;
                // inicjalizacja Slug
                string slug = "home";

                //Pobranie strony do edycji 
                PageDTO dto = db.Pages.Find(id);        //Wyszukuje id na podstawie tego co jest podane w formularzu

                dto.Title = model.Title;

                if (model.Slug != "home")
                {
                    if (string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else
                    {
                        slug = model.Slug.Replace(" ", "-").ToLower();
                    }
                }
                // Sprawdzenie unikalność strony/adresu 
                if (db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title) ||
                    db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "Strona lub adres strony już istnieje");
                }

                //modyfikacja DTO
                dto.Title = model.Title;
                dto.Body = model.Body;
                dto.Slug = slug;
                dto.HasSidebar = model.HasSidebar;

                //zapi edytowanej strony do bazy 
                db.SaveChanges();
            }
            // Ustawienie komunikatu
            TempData["SM"] = "Wyedytowałeś stronę";

            //Redirect 
            return RedirectToAction("EditPage");
        }
        // GET: Admin/Pages/Details/id
        public ActionResult Details(int id)
        {
            // Deklaracja PageWiewModel
            PageViewModel model;

            using (Db db = new Db())
            {
                // Pobranie strony o id
                PageDTO dto = db.Pages.Find(id);

                // Sprawdzenie czy strona o takim id istnieje
                if (dto == null)
                {
                    return Content("Strona o podanym id nie istnieje.");
                }

                // Inicjalizacja PageViewModel
                model = new PageViewModel(dto);

            }

            return View(model);
        }
        // GET: Admin/Pages/Delete/id
        public ActionResult Delete(int id)
        {
            using (Db db = new Db())
            {
                //Pobranie Strony do usuniecia
                PageDTO dto = db.Pages.Find(id);


                //usuwanie wybranej strony z bazy 
                db.Pages.Remove(dto);

                //zapis zmian
                db.SaveChanges();
            }
            //Przekierowanie 
                return RedirectToAction("Index");
        }
        // POST: Admin/Pages/ReorderPages
        [HttpPost]
        public ActionResult ReorderPages(int[] id)
        {
            using (Db db = new Db())
            {
                int count = 1;
                PageDTO dto;

                // Sortowanie stron, zapis na bazie 
                foreach (var pageId in id)
                {
                    dto = db.Pages.Find(pageId);
                    dto.Sorting = count;

                    db.SaveChanges();
                    count++;
                }
            }

                return View();
        }

    }
}