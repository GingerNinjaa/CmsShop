﻿using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
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

        public JsonResult IncrementProduct(int productId)
        {
            // inicjalizacja listy CartVM
            List<CartViewModel> cart = Session["cart"] as List<CartViewModel>;

            // pobieramy cart view model
            CartViewModel model = cart.FirstOrDefault(x => x.ProductId == productId);

            //zwiększamy ilośc produktu 
            model.Quantity++;

            //przygotowanie danych do JSON
            var result = new { qty = model.Quantity, price = model.Price };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DecrementProduct(int productId)
        {
            // inicjalizacja listy CartVM
            List<CartViewModel> cart = Session["cart"] as List<CartViewModel>;

            // pobieramy cart view model
            CartViewModel model = cart.FirstOrDefault(x => x.ProductId == productId);

            //zmniejszamy ilośc produktu 
            if (model.Quantity > 1)
            {
                model.Quantity--;
            }
            else
            {
                model.Quantity = 0;
                cart.Remove(model);
            }
           

            //przygotowanie danych do JSON
            var result = new { qty = model.Quantity, price = model.Price };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public void  RemoveProduct(int productId)
        {
            //Inicjalizacja listy CartViewModel
            List<CartViewModel> cart = Session["cart"] as List<CartViewModel>;

            // pobieramy cart view model
            CartViewModel model = cart.FirstOrDefault(x => x.ProductId == productId);

            //usuwamy produkt
            cart.Remove(model);

        }

        public ActionResult PayPalPartial ()
        {


            //Inicjalizacja listy CartViewModel
            List<CartViewModel> cart = Session["cart"] as List<CartViewModel>;

            return PartialView(cart);
        }
      
        [HttpPost]
        public void PlaceOrder()
        {
            // pobieramy zawartośc koszyka z sesji
            List<CartViewModel> cart = Session["cart"] as List<CartViewModel>;

            //Pobieramy nazwy użytkownika
            string username = User.Identity.Name;
            
            //deklarujemy nr zamówienia
            int orderId = 0;

            using (Db db = new Db())
            {
                //inicjalizacja OrderDTO
                OrderDTO orderDTO = new OrderDTO();

                //pobieramy user Id
                var user = db.Users.FirstOrDefault(x => x.UserName == username);
                int userId = user.Id;

                //ustawienie DTO i zapis 
                orderDTO.UserId = userId;
                orderDTO.CreatedAt = DateTime.Now;

                db.Orders.Add(orderDTO);
                db.SaveChanges();

                // pobieramy id zapisanego zamowienia
                orderId = orderDTO.OrderId;

                //inicjalizacja orderDetailDto
                OrderDetailsDTO orderDetailsDTO = new OrderDetailsDTO();

                foreach (var item in cart)
                {
                    orderDetailsDTO.OrderId = orderId;
                    orderDetailsDTO.UserId = userId;
                    orderDetailsDTO.ProductId = item.ProductId;
                    orderDetailsDTO.Quantity = item.Quantity;

                    db.OrderDetails.Add(orderDetailsDTO);
                    db.SaveChanges();
                }

            }

            // wysyłanei emaila do admina 

            var client = new SmtpClient("smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("ff1b7d185bb6ba", "eacd1da60f7b4d"),
                EnableSsl = true
            };
            client.Send("admin@example.com", "admin@example.com", "Nowe zamówienie", "Masz nowe zamówienie. Numer zamówienia " + orderId);
        

            // reset sesion
            Session["cart"] = null;



        }
    }
}