using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

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
        [HttpGet]
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

        // Post: Account/create-account
        [ActionName("create-account")]
        [HttpPost]
        public ActionResult CreateAccount(UserViewModel model)          //Rejestracja 
        {
            // sprawdzenie model state
            if (!ModelState.IsValid)
            {
                return View("CreateAccount", model);
            }

            //Sprawdzenie hasła
            if (!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Hasła do siebie nie pasują");
                return View("CreateAccount", model);
            }

            //Sprawdzenie czy nazwa użytkownika jest unikalna 
            using (Db db = new Db())
            {
                if (db.Users.Any(x => x.UserName.Equals(model.UserName)))
                {
                    ModelState.AddModelError("", "Nazwa użytkownika "+ model.UserName + " Jest już zajęta");
                    model.UserName = "";
                    return View("CreateAccount", model);
                }
                //utworzenie użytkownika 
                UserDTO usserDTO = new UserDTO()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAddress = model.EmailAddress,
                    UserName = model.UserName,
                    Password = model.Password,
                };

                //dodawanie użytkownika 
                db.Users.Add(usserDTO);

                //Zapis na bazie 
                db.SaveChanges();

                // dodanie roli dla użytkownika 
                UserRoleDTO userRoleDTO = new UserRoleDTO()
                {
                    UserId = usserDTO.Id,
                    RoleId = 2,
                };

                //Dodanie roli 
                db.UserRoles.Add(userRoleDTO);

                //zapis na bazie 
                db.SaveChanges();

                //TempData komunikat
                TempData["SM"] = "Jestes tereaz zarejestrowany i możesz się zalogować.";
            }

            return Redirect("~/account/login");
        }

        // POST: Account/login
        [HttpPost]
        public ActionResult Login(LoginUserViewModel model)
        {
            // sprawdzanie czy wszystko jest dobrze wypełnione (modelstate)
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //Sprawdamyu czy uzytkownik jest prawidłowy 
            bool isvalid = false;
            using (Db db = new Db())
            {
                if (db.Users.Any(x => x.UserName.Equals(model.UserName) && x.Password.Equals(model.Password)))
                {
                    isvalid = true;
                }
            }
            if (!isvalid)
            {
                ModelState.AddModelError("", "Nieprawidłowa nazwa użytkownika lub hasło");
                return View(model);
            }
            else
            {
                FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                return Redirect(FormsAuthentication.GetRedirectUrl(model.UserName, model.RememberMe));
            }
            
        }

        // GET: Account/logout
        public ActionResult Logout()
        {
            // wylogowanie sie 
            FormsAuthentication.SignOut();

            return Redirect("~/account/login");
        }

    }
}