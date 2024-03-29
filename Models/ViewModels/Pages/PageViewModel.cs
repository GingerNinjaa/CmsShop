﻿using CmsShop.Models.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShop.Models.ViewModels.Pages
{
    public class PageViewModel
    {

        public PageViewModel()
        {
        }
        public PageViewModel(PageDTO row)
        {
            Id = row.Id;
            Title = row.Title;
            Slug = row.Slug;
            Body = row.Body;
            Sorting = row.Sorting;
            HasSidebar = row.HasSidebar;

        }
        public int Id { get; set; }
        [Required]                                          // To oznacza ze pole jest wymagane
        [StringLength(50,MinimumLength =3)]                 // Maksymalna wartość tego pola to 50, a minimalna to 3
        [Display(Name ="Tytuł Strony")]                     
        public string Title { get; set; }
        [Display(Name = "Adres Strony")]
        public string Slug { get; set; }
        [Required]
        [StringLength(int.MaxValue, MinimumLength = 3)]     // Maksymalna wartość tego pola to int.MaxValue(Ponieważ w DB ustawiłem MAX),
        [Display(Name = "Zawartość Strony")]                // a minimalna to 3
        [AllowHtml]                                         // [AllowHtml]    <= Akceptuje czysty HTML
        public string Body { get; set; }
        
        public int Sorting { get; set; }
        [Display(Name = "Pasek Boczny")]
        public bool HasSidebar { get; set; }
    }
}