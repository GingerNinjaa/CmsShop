using CmsShop.Models.ViewModels.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShop.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {

            return View();
        }
        public ActionResult CartPartial()
        {
            // inicjalizacja cartViewModel
            CartViewModel model = new CartViewModel();

            //inicjalizacja ilość i cena 
            int qty = 0;
            decimal price = 0;

            //Sprawdzamy czy mamy dane koszyka zapisane w sesji 
            if (Session["cart"] != null)
            {
                // Pobieranie wartośći z sesji
                var list = (List<CartViewModel>)Session["cart"];

                foreach (var item in list)
                {
                    qty += item.Quantity;
                    price += item.Quantity * item.Price;
                }
            }
            else
            {
                // ustawiamy ilośc i cena na 0
                qty =  0;
                price = 0;
            }

            return PartialView(model);
        }
    }
}