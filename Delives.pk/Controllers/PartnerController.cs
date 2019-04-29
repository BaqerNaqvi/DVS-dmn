using Services.Models;
using Services.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Delives.pk.Controllers
{
    public class PartnerController : Controller
    {
       
        [HttpPost]
        public ActionResult PartnerCreate(ListItemLocal model) 
        {
            ListService.Create(model);
            return View(model);
        }

        [HttpGet]
        public ActionResult PartnerCreate()
        {
            var cats = ListService.GetCategories(true);
            return View(new ListItemLocal { Categoreis= cats});
        }


        [HttpGet]
        public ActionResult View()
        {
            var items = ListService.GetItemsForList_AdminPanel();
            return View(items);
        }

    }
}