using Services.Models;
using Services.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Delives.pk.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var obj = new ListItemLocal {
                Type= 3,
                Name = "Type 3 - 2",
                Description = "No Description Given",
                Location = "32.09249466_74.08739947",
                Phone= "055-89335696",
                Rating= "4.1",
                Address= "Noshehra Road Grw",
                LogoImage= "http://via.placeholder.com/200x100",
                BgImage= "http://via.placeholder.com/200x100",
                Status= true
            };
         //   ListService.AddNewRestaurent(obj);

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
          
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}