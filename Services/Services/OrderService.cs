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
using System.Data.Entity;

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
                        DateTime = CommonService.GetSystemTime(),
                        Cords = userLoc,
                        EstimatedTime = estimatedTime,
                        SerialNo = serial,
                        PickedBy = Guid.Empty.ToString(),
                        UpdatedAt = CommonService.GetSystemTime(),
                        DeliveryCost = CommonService.GetDeliveryAmount()
                    };
                    dbContext.Orders.Add(order);

                    dbContext.OrderHistories.Add(new OrderHistory
                    {
                        OrderId = order.Id,
                        DateTime = CommonService.GetSystemTime(),
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

                return new PlaceOrderResponseModel { EstimatedTime = estimatedTime, OrderIds = orderIds, SerailNo = serial };
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

                if (list != null && list.Any())
                {
                    foreach (var o in list)
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
                    foreach (var o in order)
                    {
                        var ord = o.MappOrder();
                        orders.Add(ord);
                    }
                }
                return new GetOrderBySerialNoResponse
                {
                    Orders = orders,
                    DeliveryFee = CommonService.GetDeliveryAmount(),
                    DeliveryTime = 50
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
                    order.UpdatedAt = CommonService.GetSystemTime();

                    var currentHis = dbContext.OrderHistories.FirstOrDefault(d => d.OrderId.ToString() == model.OrderId && d.IsCurrent);
                    if (currentHis != null)
                    {
                        currentHis.IsCurrent = false;
                    }

                    dbContext.OrderHistories.Add(new OrderHistory
                    {
                        OrderId = order.Id,
                        DateTime = CommonService.GetSystemTime(),
                        Status = model.NewStatus,
                        IsCurrent = true,
                        Comments= model.Comments
                    });
                    dbContext.SaveChanges();

                    // generate notifications
                    NotificationService.ProcessNotificationRequest(newStatus, order.Id);
                    return true;
                }
                return false;
            }
        }

        public static UpdateOrderResponse EditOrder_Admin(OrderLocal source)
        {
            var response = new UpdateOrderResponse { };
            using (var dbContext = new DeliversEntities())
            {
                var dbOrder = dbContext.Orders.FirstOrDefault(o => o.Id == source.Id);
                if (dbOrder != null)
                {
                    var canEditOrder = true;
                    #region EDIT
                    if (dbOrder.Status == OrderHistoryEnu.Deliverd.Value || dbOrder.Status == OrderHistoryEnu.PickedUp.Value)
                    {
                        canEditOrder = false;
                    }

                    #region STATUS
                    if (dbOrder.Status != source.Status)
                    {
                        var dbOrderStatus = OrderService.GetOrderCurrentStatus(source.Id.ToString());
                        var updatedOrderStatus = OrderHistoryEnu.GetOrderStatus(source.Status);

                        if (updatedOrderStatus.Order - dbOrderStatus.Order > 1)
                        {
                            response.Status = false;
                            response.Error = "Invalid order status";
                            return response;
                        }

                        // UPDATE STATUS
                        ChangeOrderStatus(new ChangeOrderStatusRequesrModel {
                             OrderId= source.Id.ToString(),
                            NewStatus= source.Status,
                            UserId = null,
                            Comments= source.Comments
                        });
                    }

                    #endregion

                    var newHist = new OrderHistory
                    {
                        OrderId = dbOrder.Id,
                        DateTime = CommonService.GetSystemTime(),
                        IsCurrent = false,
                        Status= OrderHistoryEnu.OrderEdited.Value,
                        Comments= source.Comments
                    };
                    var isAddHis = false;
                    var comm = "";

                   
                    if (!string.IsNullOrEmpty(source.Address) && source.Address.ToLower() != dbOrder.Address.ToLower())
                    {
                        if (canEditOrder)
                        {
                            comm = "address changed from '" + dbOrder.Address + "' TO '" + source.Address + "' & ";
                            dbOrder.Address = source.Address;
                            isAddHis = true;
                        }
                        else
                        {
                            response.Status = false;
                            response.Error = "Can not change address in current status: "+dbOrder.Status;
                            return response;
                        }                     
                    }
                    if (!string.IsNullOrEmpty(source.Instructions) && source.Instructions.ToLower() != dbOrder.Instructions.ToLower())
                    {
                        dbOrder.Instructions = source.Instructions;
                    }
                    if (!string.IsNullOrEmpty(source.PickedBy) && source.PickedBy.ToLower() != dbOrder.PickedBy.ToLower())
                    {
                        dbOrder.PickedBy = source.PickedBy;
                    }
                    if (source.DeliveryCost != dbOrder.DeliveryCost)
                    {
                        if (canEditOrder)
                        {
                            comm = comm + "delivery cost changed from " + dbOrder.DeliveryCost + " TO " + source.DeliveryCost + ".";
                            isAddHis = true;
                            dbOrder.DeliveryCost = source.DeliveryCost;
                        }
                        else
                        {
                            response.Status = false;
                            response.Error = "Can not change delivery cost in current status: " + dbOrder.Status;
                            return response;
                        }
                    }

                    if (isAddHis)
                    {
                        newHist.Comments = comm;
                        dbContext.OrderHistories.Add(newHist);
                    }
                    
                    dbContext.SaveChanges();
                    response.Status = true;
                    response.Error = "";
                    return response;
                    #endregion

                }
                else
                {
                    response.Status = false;
                    response.Error = "Invalid order Id";
                    return response;
                }
            }
        }

        public static AlterOrderResponse AlterOrder_Admin(AlterOrderRequestModel source)
        {
            using (var dbContext = new DeliversEntities())
            {
                var response = new AlterOrderResponse {
                    isSuccesss= true
                };
                var totalAmount = source.Items.Sum(i => ItemDetailsService.GetItemDetailLocalById(i.itemId).Price * i.quantity);
                if (totalAmount <= 0)
                {
                    response.isSuccesss = false;
                    response.Message = "Total amount can not be 0";
                    return response;
                }
                var order = dbContext.Orders.FirstOrDefault(o => o.Id == source.OrderId);
                if (order != null)
                {
                    if(order.Status== OrderHistoryEnu.Deliverd.Value || order.Status == OrderHistoryEnu.PickedUp.Value)
                    {
                        response.isSuccesss = false;
                        response.Message = "Can not change order with status: "+order.Status;
                        return response;
                    }

                    if (order.Amount != totalAmount)
                    {                        
                        order.Amount = totalAmount;
                        order.UpdatedAt = CommonService.GetSystemTime();
                        if (order.OrderDetails != null && order.OrderDetails.Any())
                        {
                            foreach (var det in order.OrderDetails)
                            {
                                var newQ = source.Items.FirstOrDefault(i => i.itemId == det.ItemId).quantity;
                                if (newQ != det.Quantity)
                                {
                                    det.Quantity = newQ;
                                }
                            }
                            dbContext.OrderHistories.Add(new OrderHistory { DateTime= CommonService.GetSystemTime(),
                                IsCurrent= false,
                                OrderId= source.OrderId,
                                Status= OrderHistoryEnu.OrderAltered.Value,
                                Comments= source.Comments
                            });
                            dbContext.SaveChanges();
                        }
                    }                    
                }
                return response;
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
                        od => od.Status == orderStatus.Value &&
                            od.OrderHistories.Any(str => str.Status == orderStatus.Value) && od.PickedBy == requestModel.UserId)
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
            string newStatus="";
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
                        dbO.UpdatedAt = CommonService.GetSystemTime();

                        var currentHis = dbContext.OrderHistories.FirstOrDefault(d => d.OrderId.ToString() == dbO.Id.ToString() && d.IsCurrent);
                        if (currentHis != null)
                        {
                            currentHis.IsCurrent = false;
                        }

                        dbContext.OrderHistories.Add(new OrderHistory
                        {
                            OrderId = dbO.Id,
                            DateTime = CommonService.GetSystemTime(),
                            Status = newStatus,
                            IsCurrent = true
                        });
                    }
                    if (notAvaialbeORders == 0)
                    {
                        dbContext.SaveChanges();
                        notAvaialbeORders = 0;
                        // generate notifications
                        NotificationService.ProcessNotificationRequest(newStatus, order.Id);
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
                string newStatus = "";
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
                            DateTime = CommonService.GetSystemTime(),
                            Status = OrderHistoryEnu.CanceledByRider.Value,
                            IsCurrent = false
                        });
                        dbContext.OrderHistories.Add(new OrderHistory
                        {
                            OrderId = order.Id,
                            DateTime = CommonService.GetSystemTime(),
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
                (string.IsNullOrEmpty(model.Status) || o.Status.ToLower() == model.Status.ToLower())).ToList();


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
                            od.OrderHistories.Any(str => str.Status == orderStatus.Value)
                            && od.OrderDetails.Any(det => det.ItemDetail.ListItemId.ToString() == model.RestaurantId))
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

                var list = (from od in dbContext.Orders
                            where
 //  od.DateTime.Date >= startDate.Date && od.DateTime.Date <= endDate.Date &&
 od.OrderDetails.Any(det => det.ItemDetail.ListItemId.ToString() == model.RestaurantId)
                            select od).ToList();

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

        #region ADMIN

        public static List<OrderLocal> GetAllOrders_Admin()
        {
            using (var dbContext = new DeliversEntities())
            {
                var orders = dbContext.Orders.ToList().OrderByDescending(o => o.Id).Select(o => o.MappOrder()).ToList();
                return orders;
            }
        }

        public static List<OrderLocal> GetFilteredOrders_Admin(SearchOrdersRequestModel source)
        {
            using (var dbContext = new DeliversEntities())
            {
                try
                {
                    //var results = dbContext.Orders.Where(x => (source.ListStatuses.Count == 0 || (source.ListStatuses.Any(s => s == x.Status))) &&
                    //(source.ListRiders.Count == 0 || (source.ListRiders.Any(s => x.PickedBy.Equals(s)))) &&
                    //(source.ListRestaurants.Count == 0 || (source.ListRestaurants.Any(s => x.OrderDetails.FirstOrDefault().ItemDetail.Id == s)))
                    //).ToList().OrderByDescending(o => o.Id).Select(o => o.MappOrder()).ToList();
                    var abc = dbContext.Orders.ToList();
                    var results = dbContext.Orders.Where(x => (source.ListStatuses.Count == 0)
                    && (source.ListRiders.Count == 0 || (source.ListRiders.Any(s => x.PickedBy.Equals(s))))
                    ).ToList().OrderByDescending(o => o.Id).Select(o => o.MappOrder()).ToList();
                    return results;
                }
                catch (Exception e)
                {
                    return null;
                    //throw;
                }

                //var orders = dbContext.Orders.ToList().OrderByDescending(o => o.Id).Select(o => o.MappOrder()).ToList();
            }
        }

        public static List<OrderLocal> GetFilteredOrdersSingle_Admin(SearchOrdersRequestModelSingle source)
        {
            using (var dbContext = new DeliversEntities())
            {
                try
                {
                    //var results = dbContext.Orders.Where(x => (source.ListStatuses.Count == 0 || (source.ListStatuses.Any(s => s == x.Status))) &&
                    //(source.ListRiders.Count == 0 || (source.ListRiders.Any(s => x.PickedBy.Equals(s)))) &&
                    //(source.ListRestaurants.Count == 0 || (source.ListRestaurants.Any(s => x.OrderDetails.FirstOrDefault().ItemDetail.Id == s)))
                    //).ToList().OrderByDescending(o => o.Id).Select(o => o.MappOrder()).ToList();

                    //temp code,remove asap.
                    //var abc = dbContext.Orders.ToList();
                    var results = dbContext.Orders.Where(x => (string.IsNullOrEmpty(source.Status) || x.Status == source.Status)
                    && (string.IsNullOrEmpty(source.Rider) || (source.Rider == x.PickedBy))
                    && (source.Restaurant == null || (source.Restaurant == x.OrderDetails.FirstOrDefault().RestId))

                    );

                    if (source.OrderDateFrom != null && source.OrderDateTo != null)
                    {
                        results = results.Where(x => Nullable.Compare((DbFunctions.TruncateTime(x.DateTime)), source.OrderDateFrom) > -1 && Nullable.Compare((DbFunctions.TruncateTime(x.DateTime)), source.OrderDateTo) < 1);
                    }

                    else if (source.OrderDateTo != null)
                    {
                        results= results.Where(x=> Nullable.Compare((DbFunctions.TruncateTime(x.DateTime)), source.OrderDateTo) < 1);
                    }
                    else if(source.OrderDateFrom != null)
                    {
                        results = results.Where(x => Nullable.Compare((DbFunctions.TruncateTime(x.DateTime)), source.OrderDateFrom) > -1);
                    }
                    var _filtered = results.ToList().OrderByDescending(o => o.Id).Select(o => o.MappOrder()).ToList();

                    return _filtered;
                }
                catch (Exception e)
                {
                    return null;
                    //throw;
                }

                //var orders = dbContext.Orders.ToList().OrderByDescending(o => o.Id).Select(o => o.MappOrder()).ToList();
            }
        }

        #endregion

    }
}
 