using Microsoft.AspNet.SignalR.Client;
using Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Services
{
    public static class SignalRService
    {
        private static HubConnection hubConnection=null;
        private static IHubProxy orderNotifyProxy=null;

        public static async Task ConnectToHub(string url)
        {
            //var signalRHubUrl = ConfigurationManager.AppSettings["saveImagesIn"];
            if (hubConnection==null)
            {
                hubConnection = new HubConnection(url);
            }
            if (orderNotifyProxy==null)
            {
                orderNotifyProxy = hubConnection.CreateHubProxy("NotifierHub");
            }
           
            
            //orderNotifyProxy.On<OrderLocal>("UpdateStockPrice", _order =>
            //    Console.WriteLine("New Order Placed with Id : "+_order.Id));
            await hubConnection.Start();
            
        }

        public static async Task NotifyNewOrdersReceivedAsync(List<OrderLocal> orders,string url)
        {
            await ConnectToHub(url);
            await orderNotifyProxy.Invoke("NewOrdersNotify", orders);
            //await orderNotifyProxy.Invoke("Test");
        }
    }
}
