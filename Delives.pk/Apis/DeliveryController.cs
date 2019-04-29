using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Delives.pk.Models;
using Services.Models;
using Services.Services;

namespace Delives.pk.Apis
{
    [Authorize]
    public class DeliveryController : ApiController
    {
        [System.Web.Mvc.HttpPost]
        [System.Web.Mvc.Route("api/Delivery/OrdersReadyToAssign")]  
        public ResponseModel OrdersReadyToAssign(GetOrdersListRequestModel listModel)
        {
            var response = new ResponseModel
            {
                Success = false,
                Messages = new List<string>()
            };
            if (listModel == null)
            {
                response.Messages.Add("Mandatory data can not be empty");
            }
            else if (listModel.CurrentPage <= 0 || listModel.ItemsPerPage <= 0)
            {
                response.Messages.Add("Current page/ItemsPerPage should be greater than 0");
            }
            else
            {
                try
                {
                    var orders = OrderService.OrdersReadyToAssign(listModel);
                    response.Data = orders;
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
        [Route("api/Delivery/MyOrders")]
        public ResponseModel MyDeliveredOrders_DBOY(OrderHistoryForDBoyRequesrModel model)
        {
            var response = new ResponseModel
            {
                Success = false,
                Messages = new List<string>()
            };
            if (model == null ||string.IsNullOrEmpty(model.UserId) || string.IsNullOrEmpty(model.Status))
            {
                response.Messages.Add("Data not mapped");
                response.Data = model;
            }
            else if (!CommonService.VerifyOrderStatus(model.Status))
            {
                response.Messages.Add("Invalid order status");
                response.Data = model;
            }
            else
            {
                try
                {
                    var data =OrderService.MyOrderHistoryDBoy(model);
                    response.Data = data;
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



        //Get All Orders for D-Boy - All statues
        [HttpPost]
        [Route("api/Delivery/MyOrders_All")]
        public ResponseModel GetMyAllOrders(OrderHistoryForDBoyRequesrModel model)
        {
            var response = new ResponseModel
            {
                Success = false,
                Messages = new List<string>()
            };
            if (model == null || string.IsNullOrEmpty(model.UserId))
            {
                response.Messages.Add("Data not mapped");
                response.Data = model;
            }
            else
            {
                try
                {
                    var data = OrderService.GetMyAllOrders(model);
                    response.Data = data;
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




        // [HttpPost]
        // [Route("api/Delivery/DboyInfo")]
        //  THAT API IS IN ACCOUNT API COZ IT NEEDED ACCOUNT MANAGER 


        [HttpPost]
        [Route("api/Delivery/GeOrdersByDate_Rider")]
        public ResponseModel GeOrdersByDate_Rider(GeOrdersByDate_RiderModel model)
        {
            var response = new ResponseModel
            {
                Success = false,
                Messages = new List<string>()
            };
            try
            {
                if (model == null || string.IsNullOrEmpty(model.UserId) || string.IsNullOrEmpty(model.StartDate) || string.IsNullOrEmpty(model.EndDate))
                {
                    response.Messages.Add("Data not mapped");
                    response.Data = model;
                }
                else if (!string.IsNullOrEmpty(model.Status) && !CommonService.VerifyOrderStatus(model.Status))
                {
                    response.Messages.Add("Invalid order status");
                    response.Data = model;
                }
                var data = OrderService.GetOrdersForRider_ByDate(model);
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

    }
}