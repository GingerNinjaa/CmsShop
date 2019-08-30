using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShop.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return Redirect("~/account/login");
        }
        // GET: Account/create-account
        [ActionName("create-account")]
        public ActionResult CreateAccount()
        {
            return View("CreateAccount");
        }
        // GET: Account/login
        public ActionResult Login()
        {
            // sprawdzanie czy użytkownik nie jest już zalogowany
            string userName = User.Identity.Name;

            // Jeśli jest zalogowany przekieruje go na user-profile
            if (!string.IsNullOrEmpty(userName))
                return RedirectToAction("user-profil");

            // zwracamy widok 
            return View();
        }
    }
}