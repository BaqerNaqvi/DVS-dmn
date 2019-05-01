using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Services.DbContext;
using Services.Models;
using System.Data.Entity.Spatial;
using System.Globalization;

namespace Services.Services
{
    public static class OrderService
    {
        public static PlaceOrderResponseModel Place(PlaceOrderRequestModel request, string orderBy)
        {
            DbGeography userLoc = null;
            var orderstatus = OrderHistoryEnu.Placed.Value;
            var estimatedTime = "45 minutes away";
            var orderIds = new List<string>();

            if (!string.IsNullOrEmpty(request.Cords) && request.Cords != "")
            {
                var latlng = request.Cords.Split('_').ToList();
                if (latlng.Count == 2)
                {
                    userLoc = CommonService.ConvertLatLonToDbGeography(latlng[1], latlng[0]); // lat _ lng
                }
            }


            using (var dbContext = new DeliversEntities())
            {
                foreach (var item in request.Items)
                {
                    var dbItem = dbContext.ItemDetails.FirstOrDefault(i => i.Id == item.ItemId);
                    if (dbItem != null)
                    {
                        item.RestId = dbItem.ListItemId;
                    }
                }
            }


            var rest_groups = request.Items.GroupBy(item => item.RestId).ToList();
            var serial = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
            using (var dbContext = new DeliversEntities())
            {
                foreach (var localG in rest_groups)
                {
                    var localList = localG.ToList();
                    var totalAmount = localList.Sum(i => ItemDetailsService.GetItemDetailLocalById(i.ItemId).Price * i.Quantity);
                    var order = new Order
                    {
                        Address = request.Address,
                        Instructions = request.Instructions,
                        Status = orderstatus,
                        OrderBy = orderBy,
                        Amount = totalAmount,
                        DateTime = DateTime.Now,
                        Cords = userLoc,
                        EstimatedTime = estimatedTime,
                        SerialNo = serial,
                        PickedBy= Guid.Empty.ToString(),
                        UpdatedAt= DateTime.Now,
                        DeliveryCost=80
                    };
                    dbContext.Orders.Add(order);

                    dbContext.OrderHistories.Add(new OrderHistory
                    {
                        OrderId = order.Id,
                        DateTime = DateTime.Now,
                        Status = orderstatus,
                        IsCurrent = true
                    });
                    foreach (var item in localList)
                    {
                        var itmObj = new OrderDetail
                        {
                            OrderId = order.Id,
                            ItemId = item.ItemId,
                            Quantity = item.Quantity,
                            RestId = item.RestId
                        };
                        dbContext.OrderDetails.Add(itmObj);
                    }
                    dbContext.SaveChanges();

                    // generate notification
                    NotificationService.ProcessNotificationRequest(orderstatus, order.Id);
                    orderIds.Add(order.Id.ToString());

                }

                return new PlaceOrderResponseModel { EstimatedTime= estimatedTime, OrderIds= orderIds, SerailNo= serial};
            }
        }

        public static GetOrdersResponseModel OrdersReadyToAssign(GetOrdersListRequestModel requestModel)
        {
            using (var dbContext = new DeliversEntities())
            {
                requestModel.CurrentPage--;
                var response = new GetOrdersResponseModel();
                var list = dbContext.Orders
                    .Where(
                        od => od.Status == OrderHistoryEnu.ConfirmedByRestaurant.Value && 
                            od.OrderHistories.Any(str => str.Status == OrderHistoryEnu.ConfirmedByRestaurant.Value && str.IsCurrent))
                    .ToList();

                DbGeography riderLoc = null;
                List<string> latlng = new List<string>();
                if (!string.IsNullOrEmpty(requestModel.Cords) && requestModel.Cords != "")
                {
                    latlng = requestModel.Cords.Split('_').ToList();
                    if (latlng.Count == 2)
                    {
                        riderLoc = CommonService.ConvertLatLonToDbGeography(latlng[1], latlng[0]); // lat _ lng
                    }
                }

                var inRangeOrders = new List<Order>();

                if(list!=null && list.Any())
                {
                    foreach(var o in list)
                    {
                        var dist = CommonService.GetDistance((double)riderLoc.Latitude, (double)riderLoc.Longitude, Convert.ToDouble(o.Cords.Latitude), Convert.ToDouble(o.Cords.Longitude));
                        //if ((int)dist < Convert.ToInt16(10))
                        {
                            inRangeOrders.Add(o);
                        }
                    }
                }

                if (inRangeOrders.Any())
                {
                    var take = list.Skip(requestModel.CurrentPage * requestModel.ItemsPerPage).
                        Take(requestModel.ItemsPerPage).ToList();
                    if (take.Any())
                    {
                        var finals = take.Select(obj => obj.MappOrderWaitingForPickup()).ToList();
                        response.Orders = finals;
                    }
                }
                response.ItemsPerPage = requestModel.ItemsPerPage;
                response.CurrentPage++;
                response.TotalItems = list.Count;
                return response;
            }
        }

        public static OrderLocal GetOrderDetails(string orderId)
        {
            using (var dbContext = new DeliversEntities())
            {
                var order = dbContext.Orders.FirstOrDefault(o => o.Id.ToString() == orderId);
                if (order != null)
                {
                   return order.MappOrder();
                }
                return null;
            }
        }


        public static GetOrderBySerialNoResponse GetOrderDetailsBySerialNo(string serialNo)
        {
            var orders = new List<OrderLocal>();
            using (var dbContext = new DeliversEntities())
            {
                var order = dbContext.Orders.Where(o => o.SerialNo.ToString() == serialNo);
                if (order.Any())
                {
                   foreach(var o in order)
                    {
                        var ord = o.MappOrder();
                        orders.Add(ord);
                    }
                }
                return new GetOrderBySerialNoResponse {
                    Orders= orders,
                    DeliveryFee= 80,
                    DeliveryTime= 50 

                };
            }
        }


        public static bool ChangeOrderStatus(ChangeOrderStatusRequesrModel model)
        {
            string newStatus;
            using (var dbContext = new DeliversEntities())
            {
                var order = dbContext.Orders.FirstOrDefault(o => o.Id.ToString() == model.OrderId);
                if (order != null)
                {
                    newStatus = model.NewStatus;

                    order.Status = model.NewStatus;
                    order.UpdatedAt = DateTime.Now;

                    var currentHis = dbContext.OrderHistories.FirstOrDefault(d => d.OrderId.ToString() == model.OrderId && d.IsCurrent);
                    if (currentHis != null)
                    {
                        currentHis.IsCurrent = false;
                    }

                    dbContext.OrderHistories.Add(new OrderHistory
                    {
                        OrderId = order.Id,
                        DateTime = DateTime.Now,
                        Status = model.NewStatus,
                        IsCurrent= true
                    });
                    dbContext.SaveChanges();

                    // generate notifications
                    NotificationService.ProcessNotificationRequest(newStatus, order.Id);
                    return true;
                }
                return false;
            }
        }

   
        public static OrderHistoryEnu GetOrderCurrentStatus(string orderid)
        {
            using (var dbContext = new DeliversEntities())
            {
                var order = dbContext.Orders.FirstOrDefault(o => o.Id.ToString() == orderid);
                if (order != null)
                {
                    var orderStatus = OrderHistoryEnu.GetOrderStatus(order.Status);
                    return orderStatus;
                }
                return null;
            }
        }

        public static GetOrdersForStatus GetOrderForStatus(GetOrdersForStatusRequestModel model)
        {
            var response = new GetOrdersForStatus();
            model.CurrentPage--;
            using (var dbContext = new DeliversEntities())
            {
                var list = dbContext.Orders
                    .Where(
                        od => od.Status.ToLower() == model.Status).ToList();

                if (list.Any())
                {
                    var take = list.Skip(model.CurrentPage * model.ItemsPerPage).
                        Take(model.ItemsPerPage).ToList();
                    if (take.Any())
                    {
                        var finals = take.Select(obj => obj.MappOrder()).ToList();
                        response.Orders = finals;
                    }
                }
                response.ItemsPerPage = model.ItemsPerPage;
                response.CurrentPage++;
                response.TotalItems = list.Count;
                return response;
            }
        }

        #region Delivery Boy

        /// <summary>
        /// Get Orders for D-Boy with specific Status
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        public static DeliveredOrdersDBoyResponseModel MyOrderHistoryDBoy(OrderHistoryForDBoyRequesrModel requestModel)
        {
            using (var dbContext = new DeliversEntities())
            {
                var response = new DeliveredOrdersDBoyResponseModel();

                var orderStatus = OrderHistoryEnu.GetOrderStatus(requestModel.Status);

                var list = dbContext.Orders
                    .Where(
                        od => od.Status== orderStatus.Value && 
                            od.OrderHistories.Any(str => str.Status == orderStatus.Value) && od.PickedBy==requestModel.UserId)
                    .ToList();

                if (list.Any())
                {
                    var finals = list.Select(obj => obj.MappOrder()).ToList();
                    response.Orders = finals;
                }
                return response;
            }
        }


        /// <summary>
        /// Get My All Orders 
        /// </summary>
        public static DeliveredOrdersDBoyResponseModel GetMyAllOrders(OrderHistoryForDBoyRequesrModel requestModel)
        {
            using (var dbContext = new DeliversEntities())
            {
                var response = new DeliveredOrdersDBoyResponseModel();

                var list = dbContext.Orders
                    .Where(
                        od =>
                            od.PickedBy == requestModel.UserId)
                    .ToList();

                if (list.Any())
                {
                    var finals = list.Select(obj => obj.MappOrder()).ToList();
                    response.Orders = finals;
                }
                return response;
            }
        }

        public static ApplyRiderResponse ApplyToOrder_ByRider(ApplyByRiderRequestmodel model)
        {
            string newStatus;
            using (var dbContext = new DeliversEntities())
            {
                var order = dbContext.Orders.FirstOrDefault(o => o.Id.ToString() == model.OrderId);
                if (order != null)
                {
                  //  var combinedOrders = dbContext.Orders.Where(o => o.SerialNo == order.SerialNo).ToList();
                  //  var ifSomeoneAppliedAlready = combinedOrders.Any(o => o.Status != OrderHistoryEnu.ConfirmedByRestaurant.Value);
                    var ifSomeoneAppliedAlready = order.Status != OrderHistoryEnu.ConfirmedByRestaurant.Value;
                    if (ifSomeoneAppliedAlready)
                    {
                        return new ApplyRiderResponse
                        {
                            isSuccesss = false,
                            Message = "Order has been assigned to some other rider."
                        };
                    }

                    var notAvaialbeORders = 0;

                    var combinedOrders = new List<Order> { order };  // this one

                    foreach (var dbO in combinedOrders)
                    {                       
                        newStatus = OrderHistoryEnu.ConfirmedByRider.Value;
                        if (dbO.Status != OrderHistoryEnu.ConfirmedByRestaurant.Value)
                        {
                            notAvaialbeORders++;
                            continue;
                        }

                        dbO.Status = newStatus;
                        dbO.PickedBy = model.UserId;
                        dbO.UpdatedAt = DateTime.Now;

                        var currentHis = dbContext.OrderHistories.FirstOrDefault(d => d.OrderId.ToString() == dbO.Id.ToString() && d.IsCurrent);
                        if (currentHis != null)
                        {
                            currentHis.IsCurrent = false;
                        }

                        dbContext.OrderHistories.Add(new OrderHistory
                        {
                            OrderId = dbO.Id,
                            DateTime = DateTime.Now,
                            Status = newStatus,
                            IsCurrent = true
                        });
                    }
                    if (notAvaialbeORders == 0)
                    {
                        dbContext.SaveChanges();
                        notAvaialbeORders = 0;
                        return new ApplyRiderResponse
                        {
                            isSuccesss = true,
                            Message = "Order has been assigned to you."
                        };
                    }
                    else
                    {
                        return new ApplyRiderResponse
                        {
                            isSuccesss = false,
                            Message = "Order/part of order has been assigned to some other rider."
                        };
                    }

                    // generate notifications
                      NotificationService.ProcessNotificationRequest(newStatus, order.Id);                   
                }
                return new ApplyRiderResponse
                {
                    isSuccesss = false,
                    Message = "Order does not exist."
                };
            }
        }

        public static bool CancelAppliedOrder_ByRider(ChangeOrderStatusRequesrModel model)
        {
            using (var dbContext = new DeliversEntities())
            {
                string newStatus="";
                var order = dbContext.Orders.FirstOrDefault(o => o.Id.ToString() == model.OrderId);
                if (order != null)
                {
                  //  var combinedOrders = dbContext.Orders.Where(o => o.SerialNo == order.SerialNo).ToList();
                   // foreach (var dbo in combinedOrders)
                    {
                        // CHECK IF HE APPLIED FOR THIS ORDER OT NOT
                        var isAppliedForOrder = order.PickedBy == model.UserId;
                        if (!isAppliedForOrder)
                        {
                            return false;
                        }

                        newStatus = OrderHistoryEnu.ConfirmedByRestaurant.Value;
                        order.Status = newStatus;
                        order.PickedBy = Guid.Empty.ToString();

                        var currentHis = dbContext.OrderHistories.FirstOrDefault(d => d.OrderId.ToString() == order.Id.ToString() && d.IsCurrent);
                        if (currentHis != null)
                        {
                            currentHis.IsCurrent = false;
                        }

                        dbContext.OrderHistories.Add(new OrderHistory
                        {
                            OrderId = order.Id,
                            DateTime = DateTime.Now,
                            Status = OrderHistoryEnu.CanceledByRider.Value,
                            IsCurrent = false
                        });
                        dbContext.OrderHistories.Add(new OrderHistory
                        {
                            OrderId = order.Id,
                            DateTime = DateTime.Now,
                            Status = newStatus,
                            IsCurrent = true
                        });
                        dbContext.SaveChanges();
                    }

                    // generate notifications
                      NotificationService.ProcessNotificationRequest(OrderHistoryEnu.CanceledByRider.Value, order.Id);
                      NotificationService.ProcessNotificationRequest(newStatus, order.Id);

                    return true;
                }
                return false;
            }
        }


        public static OrdersForRidersResponseModel GetOrdersForRider_ByDate(GeOrdersByDate_RiderModel model)
        {
            using (var dbContext = new DeliversEntities())
            {
                var response = new OrdersForRidersResponseModel();
                model.CurrentPage--;


                var startDate = Convert.ToDateTime(model.StartDate, CultureInfo.InvariantCulture); // 9/24/2017 9:31:34 AM
                var endDate = Convert.ToDateTime(model.EndDate, CultureInfo.InvariantCulture); // 9/24/2017 9:31:34 AM

                var localList = new List<Order>();

                var list = dbContext.Orders.Where(o => o.PickedBy == model.UserId &&
                (string.IsNullOrEmpty(model.Status) || o.Status.ToLower()==model.Status.ToLower())).ToList();
               

                if (list.Any())
                {
                    foreach (var od in list)
                    {
                        if (od.DateTime.Date >= startDate.Date && od.DateTime.Date <= endDate.Date)
                        {
                            localList.Add(od);
                        }
                    }

                    if (localList.Any())
                    {
                        var take = localList.Skip(model.CurrentPage * model.ItemsPerPage).
                            Take(model.ItemsPerPage).ToList();
                        if (take.Any())
                        {
                            var finals = take.Select(obj => obj.MappOrder()).ToList();
                            response.Orders = finals;
                        }
                    }                   
                }
                response.ItemsPerPage = model.ItemsPerPage;
                response.CurrentPage++;
                response.TotalItems = localList.Count;
                return response;
            }
        }



        #endregion

        #region Restaurant

        public static OrdersForRestaurantResponseModel GetOrdersForRestaurant(GetRestaurantOrdersForSpecificStatus model)
        {
            using (var dbContext = new DeliversEntities())
            {
                model.CurrentPage--;

                var response = new OrdersForRestaurantResponseModel();

                var orderStatus = OrderHistoryEnu.GetOrderStatus(model.Status);

                var list = dbContext.Orders
                    .Where(
                        od =>
                            od.OrderHistories.Any(str => str.Status == orderStatus.Value )
                            && od.OrderDetails.Any(det => det.ItemDetail.ListItemId.ToString()==model.RestaurantId))
                    .ToList();

                if (list.Any())
                {
                    var take = list.Skip(model.CurrentPage * model.ItemsPerPage).
                        Take(model.ItemsPerPage).ToList();
                    if (take.Any())
                    {
                        var finals = take.Select(obj => obj.MappOrder()).ToList();
                        response.Orders = finals;
                    }
                }
                response.ItemsPerPage = model.ItemsPerPage;
                response.CurrentPage++;
                response.TotalItems = list.Count;
                return response;
            }
        }


        public static OrdersForRestaurantResponseModel GetOrdersForRestaurant_ByDate(GetRestaurantOrdersForSpecificDate model)
        {
            using (var dbContext = new DeliversEntities())
            {
                model.CurrentPage--;

                var response = new OrdersForRestaurantResponseModel();

                var startDate = Convert.ToDateTime(model.StartDate, CultureInfo.InvariantCulture); // 9/24/2017 9:31:34 AM
                var endDate = Convert.ToDateTime(model.EndDate, CultureInfo.InvariantCulture); // 9/24/2017 9:31:34 AM

                var localList = new List<Order>();
              
                var list = (from od in dbContext.Orders where
                           //  od.DateTime.Date >= startDate.Date && od.DateTime.Date <= endDate.Date &&
                             od.OrderDetails.Any(det => det.ItemDetail.ListItemId.ToString() == model.RestaurantId) select od).ToList();

                if (list.Any())
                {
                    foreach (var od in list)
                    {
                        if (od.DateTime.Date >= startDate.Date && od.DateTime.Date <= endDate.Date)
                        {
                            localList.Add(od);
                        }
                    }

                    if (localList.Any())
                    {
                        var take = localList.Skip(model.CurrentPage * model.ItemsPerPage).
                            Take(model.ItemsPerPage).ToList();
                        if (take.Any())
                        {
                            var finals = take.Select(obj => obj.MappOrder()).ToList();
                            response.Orders = finals;
                        }
                    }
                }
                response.ItemsPerPage = model.ItemsPerPage;
                response.CurrentPage++;
                response.TotalItems = localList.Count;
                return response;
            }
        }






        #endregion

    }
}
