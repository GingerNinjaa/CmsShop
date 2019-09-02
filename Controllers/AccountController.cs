using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Account;
using CmsShop.Models.ViewModels.Shop;
using CmsShop.Views.Account;
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
                    RoleId = 2
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
        [Authorize]
        public ActionResult Logout()
        {
            // wylogowanie sie 
            FormsAuthentication.SignOut();

            return Redirect("~/account/login");
        }

        // GET: Account/user-profile
        [Authorize]
        public ActionResult UserNavPartial()
        {
            // Pobieramy nazwe użytkownika 
            string username = User.Identity.Name;

            // Deklarujemy model
            UserNavPartialViewModel model;

            using (Db db = new Db())
            {
                // pobieramy użytkownika 
                UserDTO dto = db.Users.FirstOrDefault(x => x.UserName == username);

                // budujemy model   
                model = new UserNavPartialViewModel()
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                };
            }


            return PartialView(model);
        }

        // GET: Account/user-profile
        [HttpGet]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile()
        {
            // pobieramy nazwe użytkownika
            string username = User.Identity.Name;

            //deklarujemy model
            UserProfileViewModel model;

            using (Db db = new Db())
            {
                //pobieramy użytkownika
                UserDTO dto = db.Users.FirstOrDefault(x => x.UserName == username);

                model = new UserProfileViewModel(dto);
            }

            return View("userProfile", model);
        }

        // POST: Account/user-profile
        [HttpPost]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile(UserProfileViewModel model)
        {
            // sprawdzenie model state
            if (!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }

            // sprawdzamy hasła 
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Hasła nie pasuja do siebie");
                    return View("userProfile", model);
                }
            }

            using (Db db = new Db())
            {
                // pobieramy nazwe użytkownika
                string username = User.Identity.Name;

                // sprawdzamy czy nazwa użytkownika jest unikalna
                if (db.Users.Where(x => x.Id != model.Id).Any(x => x.UserName == username))
                {
                    ModelState.AddModelError("", "Nazwa użytkownika" + model.UserName + "zajęta ");
                    model.UserName = "";
                    return View("UserProfile", model);
                }
                // edycja dto 
                UserDTO dto = db.Users.Find(model.Id);
                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.EmailAddress = model.EmailAddress;
                dto.UserName = model.UserName;

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    dto.Password = model.Password;
                }

                // zapis
                db.SaveChanges();
            }

            // ustawienia komunikatu
            TempData["SM"] = "Edytowałeś swój profil!";

            return Redirect("~/account/user-profile");
        }

        // GET: Account/Order
        [HttpGet]
        [Authorize(Roles="User")]
        public ActionResult Orders()
        {
            //inicjalizacja listy zamówien dla użytkownika
            List<OrderForUserViewModel> orddersForUser = new List<OrderForUserViewModel>();

            using (Db db = new Db())
            {
                // pobieramy id użytkownika 
                UserDTO user = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
                int userId = user.Id;

                //pobieramy zamowienia dla uzytkownika 
                List<OrderViewModel> orders = db.Orders.Where(x => x.UserId == userId).ToArray().Select(x => new OrderViewModel(x)).ToList();

                foreach (var order in orders)
                {
                    //inicjalizacja słownika produktów 
                    Dictionary<string, int> productsAndQty = new Dictionary<string, int>();
                    decimal total = 0m;

                    // pobieramy szczeguły zamówienia 
                    List<OrderDetailsDTO> orderDetailsDTO = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    foreach (var orderDetails in orderDetailsDTO)
                    {
                        // pobieramy produkt 
                        ProductDTO product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();

                        //pobieramy cene 
                        decimal price = product.Price;
                        //pobieramy nazwe
                        string productname = product.Name;

                        //dodajemy produkt do słownika 
                        productsAndQty.Add(productname, orderDetails.Quantity);

                        total += orderDetails.Quantity * price;
                    }
                    orddersForUser.Add(new OrderForUserViewModel()
                    {
                        OrderNumber = order.OrderId,
                        Total = total,
                        ProductsAnyQty = productsAndQty,
                        CreatedAt = order.CreatedAt

                    });
                }
            }
            return View(orddersForUser);
        }
    }
}