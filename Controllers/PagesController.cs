using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShop.Controllers
{
    public class PagesController : Controller
    {
        // GET: Index/{pages}
        public ActionResult Index(string page = "")
        {
            // ustawiamy adres naszej strony 
            if (page == "")
                page = "home";

            // delkarujemy pagevm i pageDTO
            PageViewModel model;
            PageDTO dto;
            // Sprawdzamy czy strona istnieje 
            using (Db db = new Db())
            {
                //Jeśli strona nie istnieje to przekierowujemy ją na strone główną 

                if (!db.Pages.Any(x => x.Slug.Equals(page)))                
                    return RedirectToAction("Index", new { page = "" });                
            }

            // Pobieramy pageDTO
            using (Db db = new Db())
            {
                dto = db.Pages.Where(x => x.Slug == page).FirstOrDefault();
            }

            // ustawiamy tytuł naszej strony 
            ViewBag.PageTitle = dto.Title;

            // sprawdzamy czy strona ma sidebar 
            if (dto.HasSidebar == true)
                ViewBag.Sidebar = "Tak";
            else
                ViewBag.Sidebar = "Tak";
            // inicjalizujemy pageVM
            model = new PageViewModel(dto);

            // zwracamy widok z pageVM
            return View(model);
        }

        public ActionResult PagesMenuPartial()
        {
            //Deklaracja pageViewModel
            List<PageViewModel> pageViewModelsList;

            //pobieranie stron
            using (Db db = new Db())
            {
                pageViewModelsList = db.Pages.ToArray()
                    .OrderBy(x => x.Sorting)
                    .Where(x => x.Slug != "home")
                    .Select(x => new PageViewModel(x)).ToList();
            }

            //Zwracamy pageVMList
                return PartialView(pageViewModelsList);

        }
    }
}