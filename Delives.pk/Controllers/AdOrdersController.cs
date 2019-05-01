using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Services.DbContext;
using Services.Services;

namespace Delives.pk.Controllers
{
    public class AdOrdersController : Controller
    {
        private DeliversEntities db = new DeliversEntities();

        // GET: AdOrders
        public ActionResult Index()
        {
            var orders = OrderService.GetAllOrders_Admin();
            return View(orders.ToList());
        }

        // GET: AdOrders/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // GET: AdOrders/Create
        public ActionResult Create()
        {
            ViewBag.OrderBy = new SelectList(db.AspNetUsers, "Id", "FirstName");
            return View();
        }

        // POST: AdOrders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,DateTime,Status,Address,OrderBy,Amount,EstimatedTime,Instructions,Cords,SerialNo,PickedBy,UpdatedAt,DeliveryCost")] Order order)
        {
            if (ModelState.IsValid)
            {
                db.Orders.Add(order);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.OrderBy = new SelectList(db.AspNetUsers, "Id", "FirstName", order.OrderBy);
            return View(order);
        }

        // GET: AdOrders/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            ViewBag.OrderBy = new SelectList(db.AspNetUsers, "Id", "FirstName", order.OrderBy);
            return View(order);
        }

        // POST: AdOrders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,DateTime,Status,Address,OrderBy,Amount,EstimatedTime,Instructions,Cords,SerialNo,PickedBy,UpdatedAt,DeliveryCost")] Order order)
        {
            if (ModelState.IsValid)
            {
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.OrderBy = new SelectList(db.AspNetUsers, "Id", "FirstName", order.OrderBy);
            return View(order);
        }

        // GET: AdOrders/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // POST: AdOrders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            Order order = db.Orders.Find(id);
            db.Orders.Remove(order);
            db.SaveChanges();
            return RedirectToAction("Index");
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
