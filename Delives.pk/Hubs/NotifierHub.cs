using System;
using System.Collections.Generic;
using System.Web;
using Microsoft.AspNet.SignalR;
using Services.Models;

namespace Delives.pk.Hubs
{
    public class NotifierHub: Hub
    {
        public void NewOrdersNotify(List<OrderLocal> orders)
        {
            // Call the addNewMessageToPage method to update clients.
            Clients.All.addNewOrdersToPage(orders);
        }

        public void Test()
        {
            // Call the addNewMessageToPage method to update clients.
            Clients.All.addNewOrderToPage("Hello from Ssignalr");
        }
    }
}