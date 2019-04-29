using Services.DbContext;
using Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Services
{
    public static class CustomerService
    {
        public static CustomerHistoryResponseModel GetCustomerHistory(string userid)
        {
            var response = new CustomerHistoryResponseModel();
            using (var dbContext = new DeliversEntities())
            {
                var uobj = dbContext.AspNetUsers.FirstOrDefault(u => u.Id == userid);
                if (uobj != null)
                {
                    response.CustomerName = uobj.FirstName + uobj.LastName;
                    var orders = dbContext.Orders.Where(o => o.OrderBy == userid).ToList();
                    if (orders.Any())
                    {
                       var mappedOrders =   orders.Select(od => od.MappOrder()).ToList();
                       response.Orders = mappedOrders;
                    }
                }
            }
            return response;
        }
    }
}
