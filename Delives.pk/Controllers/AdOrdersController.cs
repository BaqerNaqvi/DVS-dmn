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
using System.Threading.Tasks;
using System.Linq.Expressions;
using Services.Helpers;

namespace Delives.pk.Controllers
{

    [Authorize]
    public class AdOrdersController : Controller
    {

        [Authorize(Roles = "admin,operator,visitor")]
        public ActionResult Index()
        {
            var orders = OrderService.GetAllOrders_Admin();
            return View(orders.ToList());
        }

        [Authorize(Roles = "admin,operator,visitor")]
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var order = OrderService.GetOrderDetails(id);
            return View(order);
        }


        [Authorize(Roles = "admin,operator")]
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
        [Authorize(Roles = "admin,operator")]
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Status,Address,Instructions,PickedBy,DeliveryCost,Comments")] OrderLocal order)
        {
            var response = OrderService.EditOrder_Admin(order);
            if (response.Status)
            {
                return RedirectToAction("Index");
            }
            ModelState.AddModelError(string.Empty, response.Error);
            var refMOdel = new EditOrderModel
            {
                Order = OrderService.GetOrderDetails(order.Id.ToString()),
                OrderStatus = OrderHistoryEnu.GetAllOrderStatus(),
                Riders = UserService.GetUsersByype("0")
            };
            return View(refMOdel);
        }

        #region Filters section

        public JsonResult GetAllOrderStatuses()
        {

            var retModel = new AllOrderStatuses
            {
                Success = false
            };

            try
            {
                retModel.ListStatuses = OrderHistoryEnu.GetAllOrderStatus();
                retModel.Success = true;
            }
            catch (Exception e)
            {
                retModel.Message = "could not get order statuses.";
                throw;
            }

            return Json(retModel, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetAllRestaurants()
        {
            var retModel = new AllRestaurants
            {
                Success = false
            };

            try
            {
                retModel.ListRestaurants = ListService.GetItemsForSearch_AdminPanel();
                retModel.Success = true;
            }
            catch (Exception e)
            {
                retModel.Message = "could not get restaurants.";
                throw;
            }

            return Json(retModel, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetAllRiders()
        {
            var retModel = new AllRiders
            {
                Success = false
            };

            try
            {
                retModel.ListRiders = UserService.GetUsersByype("0");
                retModel.Success = true;
            }
            catch (Exception e)
            {
                retModel.Message = "could not get riders.";
                throw;
            }

            return Json(retModel, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        //SearchOrdersRequestModelSingle
        //        public ActionResult SearchOrders(SearchOrdersRequestModel obj)
        public ActionResult SearchOrders(SearchOrdersRequestModelSingle obj)
        {
            try
            {
                //var predicate = PredicateBuilder.BuildPredicateForOrdersSearching(obj);

                //if (obj.ListRestaurants == null)
                //{
                //    obj.ListRestaurants = new List<long?>();
                //}
                //if (obj.ListRiders == null)
                //{
                //    obj.ListRiders = new List<string>();
                //}
                //if (obj.ListStatuses == null)
                //{
                //    obj.ListStatuses = new List<string>();
                //}
                var orders = OrderService.GetFilteredOrdersSingle_Admin(obj);
                return View("~/Views/AdOrders/_OrdersTable.cshtml", orders.ToList());
            }
            catch (Exception e)
            {
                //log ex here
                //throw;
                return null;
            }
        }

        //private Expression<Func<OrderLocal, bool>> BuildPredicate()
        //{
        //    Expression<Func<OrderLocal, bool>> predicate = PredicateBuilder.False<products>();
        //}
        #endregion


        [HttpPost]
        public JsonResult AlterOrder(AlterOrderRequestModel source)
        {
            var response = OrderService.AlterOrder_Admin(source);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
    }
}
