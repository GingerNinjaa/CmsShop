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
    }
}