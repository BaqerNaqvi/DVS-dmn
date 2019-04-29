using Services.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Delives.pk.Controllers
{
    public class UserController : Controller
    {
        // GET: User
        public ActionResult Riders()
        {
            var riders = UserService.GetUsersByype("0");
            return View(riders);
        }
    }
}