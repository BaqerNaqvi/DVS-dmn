using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Services.DbContext;
using Services.Models;
using System.Configuration;
using Delives.pk.Utilities;
using System.IO;
using Services.Services;

namespace Delives.pk.Controllers
{
    public class MenuItemsController : Controller
    {
        private DeliversEntities db = new DeliversEntities();
        
        // GET: MenuItems
        public async Task<ActionResult> Index(long? id)
        {
            IQueryable<ItemDetail> itemDetails = null;
            if (id != null)
            {
                ViewBag.PartnerId = id;
                itemDetails = db.ItemDetails.Include(i => i.ListItem).Where(i => i.ListItemId == id);
            }
            else
            {
                itemDetails = db.ItemDetails.Include(i => i.ListItem);
            }
            return View(await itemDetails.ToListAsync());
        }

        // GET: MenuItems/Details/5
        public async Task<ActionResult> Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ItemDetail itemDetail = await db.ItemDetails.FindAsync(id);
            if (itemDetail == null)
            {
                return HttpNotFound();
            }
            return View(itemDetail);
        }

        // GET: MenuItems/Create
        public ActionResult Create(long? partnerId)
        {
            ViewBag.ListItemId = new SelectList(db.ListItems, "Id", "Name", partnerId);
            return View(new ItemDetailLocal() {
                ListItemId = partnerId.Value
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,Price,ListItemId,ImageFile,Status,Description,Type")] ItemDetailLocal itemDetail)
        {
            itemDetail.CreationDate = itemDetail.EditDate = CommonService.GetSystemTime();
            itemDetail.Image = "img added";
            var item = itemDetail.ItemDetailMapper();
            if (ModelState.IsValid)
            {
                db.ItemDetails.Add(item);
                await db.SaveChangesAsync();
                var relativePath = ConfigurationManager.AppSettings["saveImagesIn"];

                if (itemDetail.ImageFile != null)
                   Functions.SaveFile(itemDetail.ImageFile, relativePath, Server.MapPath(relativePath), item.ListItemId + "_Menu_" + item.Id);

                return RedirectToAction("Index", new { id = item.ListItemId });
            }

            ViewBag.ListItemId = new SelectList(db.ListItems, "Id", "Name", item.ListItemId);
            return View(itemDetail);
        }

        // GET: MenuItems/Edit/5
        public async Task<ActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ItemDetail item = await db.ItemDetails.FindAsync(id);
            ItemDetailLocal itemDetail = item.ItemDetailMapper();
            if (itemDetail == null)
            {
                return HttpNotFound();
            }
            ViewBag.ListItemId = new SelectList(db.ListItems, "Id", "Name", itemDetail.ListItemId);
            return View(itemDetail);
        }

        // POST: MenuItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Price,ListItemId,CreationDate,EditDate,Image,Status,Description,Type")] ItemDetailLocal itemDetail)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(itemDetail).State = EntityState.Modified;
        //        await db.SaveChangesAsync();
        //        return RedirectToAction("Index");
        //    }
        //    ViewBag.ListItemId = new SelectList(db.ListItems, "Id", "Name", itemDetail.ListItemId);
        //    return View(itemDetail);
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Price,ListItemId,ImageFile,Status,Description,Type")] ItemDetailLocal itemDetail)
        {
            itemDetail.EditDate = CommonService.GetSystemTime();
            itemDetail.Image = "https://via.placeholder.com/40x40";
            if (ModelState.IsValid)
            {
                var changes = db.ItemDetails.FirstOrDefault(i => i.Id == itemDetail.Id);
                changes.Name = itemDetail.Name;
                changes.Description = itemDetail.Description;
                changes.EditDate = itemDetail.EditDate;
                changes.Status = itemDetail.Status;
                changes.Type = itemDetail.Type;
                changes.Price = itemDetail.Price;
                db.Entry(changes).State = EntityState.Modified;
                var relativePath = ConfigurationManager.AppSettings["saveImagesIn"];
                if (itemDetail.ImageFile != null)
                    changes.Image = Functions.SaveFile(itemDetail.ImageFile, relativePath, Server.MapPath(relativePath), changes.ListItemId + "_Menu_" + changes.Id);
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { id = changes.ListItemId });
            }

            ViewBag.ListItemId = new SelectList(db.ListItems, "Id", "Name", itemDetail.ListItemId);
            return View(itemDetail);
        }
        // GET: MenuItems/Delete/5
        public async Task<ActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ItemDetail itemDetail = await db.ItemDetails.FindAsync(id);
            if (itemDetail == null)
            {
                return HttpNotFound();
            }
            return View(itemDetail);
        }

        // POST: MenuItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(long id, long ListItemId)
        {
            ItemDetail itemDetail = await db.ItemDetails.FindAsync(id);
            db.ItemDetails.Remove(itemDetail);
            await db.SaveChangesAsync();
            return RedirectToAction("Index", new { id = ListItemId });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        private async Task<ActionResult> CreateItem([Bind(Include = "Id,Name,Price,ListItemId,CreationDate,EditDate,Image,Status,Description,Type")] ItemDetail itemDetail)
        {
            if (ModelState.IsValid)
            {
                db.ItemDetails.Add(itemDetail);
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { id = itemDetail.ListItemId });
            }

            ViewBag.ListItemId = new SelectList(db.ListItems, "Id", "Name", itemDetail.ListItemId);
            return View(itemDetail);
        }
        private async Task<ActionResult> EditItem([Bind(Include = "Id,Name,Price,ListItemId,CreationDate,EditDate,Image,Status,Description,Type")] ItemDetail itemDetail)
        {
            if (ModelState.IsValid)
            {
                db.Entry(itemDetail).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { id = itemDetail.ListItemId });
            }
            ViewBag.ListItemId = new SelectList(db.ListItems, "Id", "Name", itemDetail.ListItemId);
            return View(itemDetail);
        }
    }
}
