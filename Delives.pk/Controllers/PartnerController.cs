using Delives.pk.Utilities;
using Services.DbContext;
using Services.Models;
using Services.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Delives.pk.Controllers
{
    public class PartnerController : Controller
    {

        private DeliversEntities db = new DeliversEntities();
        [HttpPost]
        public ActionResult PartnerCreate(ListItemLocal model)
        {
            model.Id = ListService.Create(model);
            var relativePath = ConfigurationManager.AppSettings["saveImagesIn"];
            if (model.Logo != null)
               Functions.SaveFile(model.Logo, relativePath, Server.MapPath(relativePath), model.Id + "_Logo");
            if (model.Background != null)
               Functions.SaveFile(model.Background, relativePath, Server.MapPath(relativePath), model.Id + "_Background");
            ListService.UpdateImages(model);
            return RedirectToAction("View");
        }

        [HttpGet]
        public ActionResult PartnerCreate()
        {
            var cats = ListService.GetCategories(true);
            return View(new ListItemLocal { Categoreis = cats });
        }


        [HttpGet]
        public ActionResult View()
        {
            var items = ListService.GetItemsForList_AdminPanel();
            return View(items);
        }

        // GET: MenuItems/Details/5
        public async Task<ActionResult> Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ListItemLocal listItemLocal = await ListService.DetailsAsync(id.Value);
            if (listItemLocal == null)
            {
                return HttpNotFound();
            }
            var restType = ListService.GetRestaurantType(listItemLocal.Type);
            if (restType != null)
            {
                listItemLocal.TypeName = restType.Name;
            }
            return View(listItemLocal);
        }
        // GET: MenuItems/Edit/5
        public async Task<ActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ListItem item = await db.ListItems.FindAsync(id);
            ListItemLocal itemDetail = item.MapListItem();
            if (itemDetail == null)
            {
                return HttpNotFound();
            }
            var cats = ListService.GetCategories(true);
            ViewBag.Type = new SelectList(cats, "CatId", "Name", itemDetail.Type);
            return View(itemDetail);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ListItemLocal itemDetail)
        {
            var relativePath = ConfigurationManager.AppSettings["saveImagesIn"];
            if (itemDetail.Logo != null)
               Functions.SaveFile(itemDetail.Logo, relativePath, Server.MapPath(relativePath), itemDetail.Id + "_Logo");
            if (itemDetail.Background != null)
               Functions.SaveFile(itemDetail.Background, relativePath, Server.MapPath(relativePath), itemDetail.Id + "_Background");
            itemDetail.Id = ListService.Edit(itemDetail);
            var cats = ListService.GetCategories(true);
            ViewBag.Type = new SelectList(cats, "CatId", "Name", itemDetail.Id);
            return RedirectToAction("view");

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}