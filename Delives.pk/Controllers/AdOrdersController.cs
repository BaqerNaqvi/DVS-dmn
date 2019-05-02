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
using Services.Models;

namespace Delives.pk.Controllers
{
    public class AdOrdersController : Controller
    {

        // GET: AdOrders
        public ActionResult Index()
        {
            var orders = OrderService.GetAllOrders_Admin();
            return View(orders.ToList());
        }

        // GET: AdOrders/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var order =OrderService.GetOrderDetails(id);
            return View(order);
        }


        // GET: AdOrders/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var model = new EditOrderModel
            {
                Order = OrderService.GetOrderDetails(id),
                OrderStatus = OrderHistoryEnu.GetAllOrderStatus(),
                Riders = UserService.GetUsersByype("0")
            };
            return View(model);
        }

        // POST: AdOrders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Status,Address,Instructions,PickedBy,DeliveryCost")] Order order)
        {
            var response = OrderService.EditOrder_Admin(order);
            if (response.Status)
            {
                return RedirectToAction("Index");
            }
            ModelState.AddModelError(string.Empty, response.Error);
            return View(order);
        }
    }
}
