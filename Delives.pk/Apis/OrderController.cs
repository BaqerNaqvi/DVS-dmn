using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Security;
using Delives.pk.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Services.Models;
using Services.Services;
using static Delives.pk.Models.UserMapper;

namespace Delives.pk.Apis
{
    [Authorize]
    public class OrderController : ApiController
    {
        [HttpPost]
        [Route("api/order/place")]
        public ResponseModel_PlaceOrder PlaceOrder(PlaceOrderRequestModel listModel)
        {
            var response = new ResponseModel_PlaceOrder
            {
                Success = false,
                Messages = new List<string>()
            };
            if (listModel == null || listModel.Items == null || listModel.Items.Count==0 && string.IsNullOrEmpty(listModel.Cords))     
            {
                response.Messages.Add("Data not mapped");
                response.Data = listModel;
            }
            else if (listModel.Cords.Split('_').Length != 2)
            {
                response.Messages.Add("Invalid Cord format. Please specify in Lat_Lang .i.e. '32.202895_74.176716'");
                response.Data = listModel;
            }
            else
            {
                try
                {
                    var orderById = listModel.OrderPlacedById;
                    if (string.IsNullOrEmpty(listModel.OrderPlacedById))
                    {
                        var contextnew = new ApplicationDbContext();
                        var userStore = new UserStore<ApplicationUser>(contextnew);
                        var userManager = new UserManager<ApplicationUser>(userStore);
                        var user = userManager.FindByName(User.Identity.Name);
                        orderById = user.Id;
                    }
                   

                    var item = OrderService.Place(listModel, orderById);
                    response.Data = item.EstimatedTime;
                    response.OrderId = item.OrderIds;
                    response.SerialNo = item.SerailNo;
                    response.Messages.Add("Success");
                    response.Success = true;
                }
                catch (Exception excep)
                {
                    response.Messages.Add("Something bad happened.");
                }
            }
            return response;
        }


        [HttpPost]
        [Route("api/order/getDetails")]
        public ResponseModel GetOrderDetails(GetOrderDetailsRequest model)
        {
            var response = new ResponseModel
            {
                Success = false,
                Messages = new List<string>()
            };
            if (model == null || string.IsNullOrEmpty(model.OrderId))
            {
                response.Messages.Add("Data not mapped");
                response.Data = model;
            }
           
            else
            {
                try
                {                   
                    var order = OrderService.GetOrderDetails(model.OrderId);
                    response.Data = order;
                    response.Messages.Add("Success");
                    response.Success = true;
                }
                catch (Exception excep)
                {
                    response.Messages.Add("Something bad happened.");
                }
            }
            return response;
        }



        [HttpPost]
        [Route("api/order/getDetailsBySerialNo")]
        public ResponseModel GetOrderDetailsBySerialNo(GetOrderDetailsBySerialNoRequest model)
        {
            var response = new ResponseModel
            {
                Success = false,
                Messages = new List<string>()
            };
            if (model == null || string.IsNullOrEmpty(model.SerialNo))
            {
                response.Messages.Add("Data not mapped");
                response.Data = model;
            }

            else
            {
                try
                {
                    var order = OrderService.GetOrderDetailsBySerialNo(model.SerialNo);
                    response.Data = order;
                    response.Messages.Add("Success");
                    response.Success = true;
                }
                catch (Exception excep)
                {
                    response.Messages.Add("Something bad happened.");
                }
            }
            return response;
        }


        [HttpPost]
        [Route("api/order/estimatedDeliveryCharges")]
        public ResponseModel GetDeliveryCharges(PlaceOrderRequestModel listModel)
        {
            var response = new ResponseModel
            {
                Success = false,
                Messages = new List<string>()
            };
            if (listModel == null || listModel.Items == null || listModel.Items.Count == 0 && string.IsNullOrEmpty(listModel.Cords))
            {
                response.Messages.Add("Data not mapped");
                response.Data = listModel;
            }
            else if (listModel.Cords.Split('_').Length != 2)
            {
                response.Messages.Add("Invalid Cord format. Please specify in Lat_Lang .i.e. '32.202895_74.176716'");
                response.Data = listModel;
            }
            else
            {
                try
                {                   
                    response.Data = new { DeliveryAmount=80, DeliveryTime=50};
                    response.Messages.Add("Success");
                    response.Success = true;
                }
                catch (Exception excep)
                {
                    response.Messages.Add("Something bad happened.");
                }
            }
            return response;
        }


        [HttpPost]
        [Route("api/order/updateStatus")]
        public ResponseModel UpdateOrderStatus(ChangeOrderStatusRequesrModel model)
        {
            var response = new ResponseModel
            {
                Success = false,
                Messages = new List<string>()
            };
            if (model == null || string.IsNullOrEmpty(model.OrderId) || string.IsNullOrEmpty(model.NewStatus) || string.IsNullOrEmpty(model.UserId))
            {
                response.Messages.Add("Data not mapped");
                response.Data = model;
            }
             else if (!CommonService.VerifyOrderStatus(model.NewStatus))
            {
                response.Messages.Add("Invalid order status");
                response.Data = model;
            }
            else
            {
                try
                {
                    var currentOrderStatus = OrderService.GetOrderCurrentStatus(model.OrderId);
                    var newOrderStatus = OrderHistoryEnu.GetOrderStatus(model.NewStatus);

                    if(currentOrderStatus==null || newOrderStatus == null)
                    {
                        response.Success = false;
                        response.Messages.Add("Invalid orderId/Order Status.");
                        response.Data = model;
                        return response;
                    }
                    else if (currentOrderStatus.Value == newOrderStatus.Value)
                        {
                            response.Success = false;
                            response.Messages.Add("Current status already is : "+currentOrderStatus.Value);
                            response.Data = model;
                            return response;
                        }

                    if (newOrderStatus.Order - currentOrderStatus.Order > 1)
                    {
                        response.Success = false;
                        response.Messages.Add("Invalid order status shifting. Current status is :" + currentOrderStatus.Value);
                        response.Data = currentOrderStatus.Value + " -> " + newOrderStatus.Value;
                        return response;
                    }

                    response.Success = OrderService.ChangeOrderStatus(model);
                    if (response.Success)
                    {
                        response.Data = "status has been changed";
                    }
                    else
                    {
                        response.Data = "Order does not exist with id "+model.OrderId;
                    }
                    response.Messages.Add("Success");
                }
                catch (Exception excep)
                {
                    response.Messages.Add("Something bad happened.");
                }
            }
            return response;
        }

        [HttpPost]
        [Route("api/order/GetOrdersForStatus")]
        public ResponseModel GetOrdersForStatus(GetOrdersForStatusRequestModel model)
        {
            var response = new ResponseModel
            {
                Success = false,
                Messages = new List<string>()
            };
            try
            {
                if (model == null ||  string.IsNullOrEmpty(model.Status))
                {
                    response.Messages.Add("Data not mapped");
                    response.Data = model;
                }
                var data = OrderService.GetOrderForStatus(model);
                response.Data = data;
                response.Success = true;
                response.Messages.Add("Success");
            }
            catch (Exception excep)
            {
                response.Messages.Add("Something bad happened.");
            }
            return response;
        }

        #region Restaurant

        [HttpPost]
        [Route("api/order/GetRestOrders")]
        public ResponseModel GetOrdersForParticularStatus(GetRestaurantOrdersForSpecificStatus model)
        {
            var response = new ResponseModel
            {
                Success = false,
                Messages = new List<string>()
            };
            try
            {
                if (model == null || string.IsNullOrEmpty(model.RestaurantId) || string.IsNullOrEmpty(model.Status))
                {
                    response.Messages.Add("Data not mapped");
                    response.Data = model;
                }
                var data = OrderService.GetOrdersForRestaurant(model);
                response.Data = data;
                response.Success = true;
                response.Messages.Add("Success");
            }
            catch (Exception excep)
            {
                response.Messages.Add("Something bad happened.");
            }
            return response;
        }

        [HttpPost]
        [Route("api/order/GetRestOrdersByDate")]
        public ResponseModel GetRestOrdersByDate(GetRestaurantOrdersForSpecificDate model)
        {
            var response = new ResponseModel
            {
                Success = false,
                Messages = new List<string>()
            };
            try
            {
                if (model == null || string.IsNullOrEmpty(model.RestaurantId) || string.IsNullOrEmpty(model.StartDate) || string.IsNullOrEmpty(model.EndDate))
                {
                    response.Messages.Add("Data not mapped");
                    response.Data = model;
                }
                var data = OrderService.GetOrdersForRestaurant_ByDate(model);
                response.Data = data;
                response.Success = true;
                response.Messages.Add("Success");
            }
            catch (Exception excep)
            {
                response.Messages.Add("Something bad happened.");
            }
            return response;
        }

        #endregion

        #region RIDER

        [HttpPost]
        [Route("api/order/ApplyForOrder")]
        public ResponseModel ApplyForOrder(ApplyByRiderRequestmodel model)
        {
            var response = new ResponseModel
            {
                Success = false,
                Messages = new List<string>()
            };
            if (model == null || string.IsNullOrEmpty(model.OrderId) || string.IsNullOrEmpty(model.UserId))
            {
                response.Messages.Add("Data not mapped");
                response.Data = model;
            }           
            else
            {
                try
                {                   
                    var res = OrderService.ApplyToOrder_ByRider(model);
                    if (res.isSuccesss)
                    {
                        response.Success = true;
                        response.Messages.Add(res.Message);
                    }
                    else
                    {
                        response.Messages.Add(res.Message);
                    }
                }
                catch (Exception excep)
                {
                    response.Messages.Add("Something bad happened.");
                }
            }
            return response;
        }

        [HttpPost]
        [Route("api/order/CancelAppliedOrder")]
        public ResponseModel CancelAppliedOrder(ChangeOrderStatusRequesrModel model)
        {
            var response = new ResponseModel
            {
                Success = false,
                Messages = new List<string>()
            };
            if (model == null || string.IsNullOrEmpty(model.OrderId) || string.IsNullOrEmpty(model.UserId))
            {
                response.Messages.Add("Data not mapped");
                response.Data = model;
            }
            else
            {
                try
                {
                   
                    response.Success = OrderService.CancelAppliedOrder_ByRider(model);
                    if (response.Success)
                    {
                        response.Messages.Add("Order Canceled");
                    }
                    else
                    {
                        response.Messages.Add("Could not cancel the order");
                    }
                    response.Data= model;
                }
                catch (Exception excep)
                {
                    response.Messages.Add("Something bad happened.");
                }
            }
            return response;
        }

        #endregion

    }
}
