using Delives.pk.Models;
using Services.Models;
using Services.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Delives.pk.Apis
{
    public class CustomerController : ApiController
    {
        [HttpPost]
        [Route("api/customer/ordersHistory")]
        public ResponseModel CustomerOrderHistoy(OrderHistoryForCustomerRequesrModel model)
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
                    var data = CustomerService.GetCustomerHistory(model.UserId);
                    response.Data = data;
                    response.Messages.Add("Success");
                    response.Success = true;
                }
                catch (Exception excep)
                {
                    response.Data = model;
                    response.Messages.Add("Something bad happened.");
                }
            }
            return response;
        }
    }
}