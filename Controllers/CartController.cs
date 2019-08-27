using CmsShop.Models.Data;
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
            //inicjalizacja koszyka 
            var cart = Session["cart"] as List<CartViewModel> ?? new List<CartViewModel>();

            //Sprawdzamy czy nasz koszyk jest pusty
            if (cart.Count == 0 || Session["cart"] == null)
            {
                ViewBag.Message = "Twój koszyk jest pusty";
                return View();
            }

            // obliczenie wartości  podsumowania koszyka i przekazanie do ViewBag
            decimal total = 0m;

            foreach (var item in cart)
            {
                total += item.Total;
            }

            ViewBag.GrandTotal = total;

            return View(cart);
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

                model.Quantity = qty;
                model.Price = price;
            }
            else
            {
                // ustawiamy ilośc i cena na 0
                qty = 0;
                price = 0m;
            }

            return PartialView(model);
        }

        public ActionResult AddToCartPartial(int id)
        {
            //Inicjalizacja CartVMlist
            List<CartViewModel> cart = Session["cart"] as List<CartViewModel> ?? new List<CartViewModel>();

            // inicjalizacja CartViewModel
            CartViewModel model = new CartViewModel();

            using (Db db = new Db())
            {
                // pobieramy produkt 
                ProductDTO product = db.Products.Find(id);


                //sprawdzamy czy ten produkt jest w już w koszyku
                var productInCart = cart.FirstOrDefault(x => x.ProductId == id);

                // w zalezności od tego czy produkt jest w koszyku go dodajemy lub zwiekszamy ilośc 
                if (productInCart == null)
                {
                    cart.Add(new CartViewModel()
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1,
                        Price = product.Price,
                        Image = product.ImageName
                    });
                }
                else
                {
                    productInCart.Quantity++;
                }
            }
            // pobieramy  całkowite wartość ilości i ceny i dodajemy do modelu 
            int qty = 0;
            decimal price = 0m;
            foreach (var item in cart)
            {
                qty += item.Quantity;
                price += item.Quantity * item.Price;
            }

            model.Quantity = qty;
            model.Price = price;

            //Zapis w sesji
            Session["cart"] = cart;

            return PartialView(model);
        }
    }
}