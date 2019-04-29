using Newtonsoft.Json.Linq;
using Services.DbContext;
using Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Services.Services
{
    public static class NotificationService
    {
        public static bool AddNewToken(NotificationRegisModel source)
        {           
            using (var dbContext = new DeliversEntities())
            {
                var tokens = dbContext.Tokens.Where(t => t.AppId == source.AppId && t.UserId == source.UserId).ToList();
                if(tokens!=null && tokens.Any())
                {
                    foreach(var tok in tokens)
                    {
                        tok.Token1 = source.Token;
                        tok.DateTime = DateTime.Now;
                    }
                    dbContext.SaveChanges();
                }
                else
                {
                    var obj = new Token
                    {
                        AppId = source.AppId,
                        Token1 = source.Token,
                        UserId = source.UserId,
                        DateTime = DateTime.Now
                    };
                    dbContext.Tokens.Add(obj);
                    dbContext.SaveChanges();
                }               
                return true;
            }
        }

        public static GetAllNotificationResponseModel GetAllNotifications(NotificationGetModel requestModel)
        {
            requestModel.CurrentPage--;
            var response = new GetAllNotificationResponseModel();
            using (var dbContext = new DeliversEntities())
            {
                var notifs = dbContext.Notifications.Where(no => no.UserId == requestModel.UserId
                && no.AppId == requestModel.AppId).ToList();

                var take = notifs.Skip(requestModel.CurrentPage * requestModel.ItemsPerPage).
                       Take(requestModel.ItemsPerPage).ToList();

                response.Notifications = take;
                response.ItemsPerPage = requestModel.ItemsPerPage;
                response.CurrentPage++;
                response.TotalItems = notifs.Count;

                return response;
            }
        }

        public static List<SendNotiResponseModel> SendNotification(NotificationSendModel model)
        {
            var response = new List<SendNotiResponseModel>();

           
            using (var dbContext = new DeliversEntities())
            {
                //#region Confirmed 
                //if (model.Action == NotificationEnum.confirmed)
                //{
                //    var allDevliveryBoyes = dbContext.AspNetUsers.Where(o => o.Type == 0 && (o.Status ?? false) && (o.IsApproved ?? false)).ToList();
                //    if (allDevliveryBoyes != null && allDevliveryBoyes.Any())
                //    {
                //        model.UserIds = new List<string>();
                //        model.UserIds = allDevliveryBoyes.Select(o => o.Id.ToString()).ToList();
                //    }
                //}
                
                //if(model.UserIds==null || !model.UserIds.Any())
                //{
                //    var tempoRes = new SendNotiResponseModel
                //    {
                //        UserId = "0",
                //        AppId = 0000,
                //        Token = new List<string> { "NOTHING"},
                //        Response = "No active delivery boy found"
                //    };
                //    response.Add(tempoRes);
                //    return response;
                //}
                 
                //#endregion

                foreach (var u in model.UserIds)
                {
                    var tempoRes = new SendNotiResponseModel
                    {
                        UserId = u,
                        AppId= model.AppId,
                        Token= new List<string>(),
                        Response= ""
                    };

                    var tokens = new List<Token>();

                    var user = dbContext.AspNetUsers.FirstOrDefault(obj => obj.Id == u);
                    if (user != null)
                    {
                       
                        var toks = dbContext.Tokens.Where(t => t.UserId == user.Id && t.AppId == model.AppId);
                        if(toks!=null && toks.Any())
                        {
                            tokens.AddRange(toks);

                            foreach (var tok in tokens)
                            {
                                tempoRes.Token.Add(tok.Token1);
                                 tempoRes.Response = SendNotiInner(model.AppId, tok.Token1,model.Text,"Testing");

                                var tempoNoti = new Notification
                                {
                                    AppId = model.AppId,
                                    DateTime = DateTime.Now,
                                    Text= model.Text+ "at "+DateTime.Now.ToLongTimeString(),
                                    UserId = user.Id
                                };
                                dbContext.Notifications.Add(tempoNoti);
                                dbContext.SaveChanges();
                            }
                        }
                        else
                        {
                            tempoRes.Token.Add("User is valid but No Token Found for this user");
                        }
                    }
                    else
                    {
                        tempoRes.Token.Add("User is not valid");
                    }
                    response.Add(tempoRes);
                }
            }

            return response;
        }

        private static string SendNotiInner(long appId, string token, string message, string title)
        {
            var mId = DateTime.Now.Millisecond;
            var jGcmData = new JObject();
            var jData = new JObject();
            var url = new Uri("https://fcm.googleapis.com/fcm/send");
            string auth = null;
            if (appId == 1)  // order app
            {
                auth = "AAAA3LphIJw:APA91bGkmGxenbS_Tlli1fHbNJuYhJvhnsRf2zH1u9pa3zvAQ3mNuewN69G8MtLz6PJkJ6Ksmklo4WpBV5tGyGvue6LNMPNc9mjYv10g90Jdq8HvesE1XJP-lQNeoDIQYb__-b4oczE1";
            }
            else if (appId == 0)// delivery/rider app
            {
                auth = "AAAAZhLRgQw:APA91bGav1qNIlqd3Q7VJUZ6vjzECSqVsoB0-xBxJljlkyrTP_mq5DdwjPIUjvGMslJEKZ6NmsgqXdkJLuoejtV8oMngLA7AJ2WmCZwLPL_8uNh7r50lkdyg1Pds2yO9JriSldXqOyWo";
            }
            else if (appId == 2)// restaurant app
            {
                auth = "AAAAM2X6b1M:APA91bHxOTofAgdrqoSjXDiKnT7eHZaE5fSBLegkAKcrr8TZ4gyURyCBp3Wt2Qx4Tmd8lbaTa8JZksmDwyz6yWP7VO-Wl-5l9BDDbVw395piZ3-sWoGPC4EVV7J5BWtzwRfWQrdjptWm";
            }


            jData.Add("message", message);
            jData.Add("mid", mId);
            jData.Add("title", title);
            jGcmData.Add("to", token);

            jGcmData.Add("data", jData);
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));

                    client.DefaultRequestHeaders.TryAddWithoutValidation(
                        "Authorization", "key="+auth);

                    Task.WaitAll(client.PostAsync(url,
                        new StringContent(jGcmData.ToString(), Encoding.Default, "application/json"))
                            .ContinueWith(response =>
                            {
                                var xxx = response.Result.Content.ReadAsStringAsync();
                                using (var dbContext = new DeliversEntities())
                                {
                                    try
                                    {
                                        dbContext.Notifications.Add(new Notification
                                        {

                                            AppId = appId,
                                            DateTime = DateTime.Now,
                                            Text = message,
                                            UserId = "0beab117-d8ef-4a8e-9978-b66b35a56be5"
                                        });
                                        dbContext.SaveChanges();
                                    }
                                    catch (Exception dfd)
                                    {

                                    }
                                }
                                    return xxx;
                            }));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to send GCM message:");
                Console.Error.WriteLine(e.StackTrace);
                return "Exception while sending notification";
            }
            return "Processing...";
        }


        public static bool ProcessNotificationRequest(string status, long orderId)
        {
            if (string.IsNullOrEmpty(status))
            {
                return false;
            }
            status = status.ToLower();

            #region ORDER PLACED - SEND NOTIFICATION TO RESTAURANTS 
            if (status == OrderHistoryEnu.Placed.Value.ToLower())
            {
                using (var dbContext = new DeliversEntities())
                {
                    var order = dbContext.Orders.FirstOrDefault(o => o.Id == orderId);
                    if (order != null)
                    {
                        var groups = order.OrderDetails.GroupBy(item => item.ItemDetail.ListItemId);
                        foreach (var g in groups)
                        {
                            var restKey = g.Key;
                            var mapping = dbContext.User_Rest_Map.FirstOrDefault(m => m.RestId == restKey);
                            if (mapping != null)
                            {
                                var token = dbContext.Tokens.FirstOrDefault(u => u.UserId == mapping.UserId);
                                if (token != null)
                                {
                                    SendNotiInner(2, token.Token1, "New order has been placed","New Order Arrived");
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region CANCEL ORDER BY CUSTOMER  - SEND NOTIFICATION TO RESTAURANTS 
            if (status == OrderHistoryEnu.CanceledByCustomer.Value.ToLower())
            {
                using (var dbContext = new DeliversEntities())
                {
                    var order = dbContext.Orders.FirstOrDefault(o => o.Id == orderId);
                    if (order != null)
                    {
                        var combinedOrders = dbContext.Orders.Where(o => o.SerialNo == order.SerialNo).ToList();
                        foreach(var dbo in combinedOrders)
                        {
                            var restKey = dbo.OrderDetails.FirstOrDefault().RestId;
                            var mapping = dbContext.User_Rest_Map.FirstOrDefault(m => m.RestId == restKey);
                            if (mapping != null)
                            {
                                var token = dbContext.Tokens.FirstOrDefault(u => u.UserId == mapping.UserId);
                                if (token != null)
                                {
                                    SendNotiInner(2, token.Token1, "Order no. " + orderId + " has been canceled by customer", "Order canceled");
                                }
                            }
                        }                       
                    }
                }
            }
            #endregion

            #region CANCEL ORDER BY REST - SEND NOTIFICATION TO CUSTOMER
            if (status == OrderHistoryEnu.CanceledByRestaurant.Value.ToLower())
            {
                using (var dbContext = new DeliversEntities())
                {
                    var order = dbContext.Orders.FirstOrDefault(o => o.Id == orderId);
                    if (order != null)
                    {
                        var token = dbContext.Tokens.FirstOrDefault(u => u.UserId == order.OrderBy);
                        if (token != null)
                        {
                            SendNotiInner(1, token.Token1, "Your order# "+order.Id+" has been canceled by restaurant", "Order Canceled");
                        }
                    }
                }
            }
            #endregion

            #region CONFIRMED ORDER BY REST - SEND NOTIFICATION TO CUSTOMER & RIDERS
            if (status == OrderHistoryEnu.ConfirmedByRestaurant.Value.ToLower())
            {
                using (var dbContext = new DeliversEntities())
                {
                    var order = dbContext.Orders.FirstOrDefault(o => o.Id == orderId);
                    if (order != null)
                    {
                        #region CUSTOMER
                        var token = dbContext.Tokens.FirstOrDefault(u => u.UserId == order.OrderBy);
                        if (token != null)
                        {
                            var restName = order.OrderDetails.FirstOrDefault().ItemDetail.ListItem.Name;
                            SendNotiInner(1, token.Token1, "Your order has been confirmed by " + restName, "Order Confirmed");
                        }
                        #endregion

                        #region RIDERS
                        var availableRiders = DeliveryService.GetAvaialableDeliveryBoys(order.Cords);  // customer location
                        if(availableRiders!=null && availableRiders.Any())
                        {
                            foreach(var r in availableRiders)
                            {
                                var riderToken = dbContext.Tokens.FirstOrDefault(l => l.UserId == r.Id);
                                if (riderToken != null)
                                {
                                    SendNotiInner(0, riderToken.Token1, "There's new order in your area", "New Order Available");
                                }
                            }
                        }
                        #endregion
                    }
                }
            }
            #endregion

            #region WAITING FOR PICKUP - SEND NOTIFICATION TO CUSTOMER AND RIDER

            if (status == OrderHistoryEnu.WaitingForPickup.Value.ToLower())
            {
                using (var dbContext = new DeliversEntities())
                {
                    var order = dbContext.Orders.FirstOrDefault(o => o.Id == orderId);
                    if (order != null)
                    {
                        var restName = order.OrderDetails.FirstOrDefault().ItemDetail.ListItem.Name;

                        #region CUSTOMER
                        var token = dbContext.Tokens.FirstOrDefault(u => u.UserId == order.OrderBy);
                        if (token != null)
                        {
                            SendNotiInner(1, token.Token1, "Your order ready at "+restName+" for pickup", "Order Ready For Pickup");
                        }
                        #endregion

                        #region RIDERS
                        var riderToken = dbContext.Tokens.FirstOrDefault(l => l.UserId == order.PickedBy);
                        if (riderToken != null)
                        {
                            SendNotiInner(0, riderToken.Token1, "Your order is ready for pick up at "+restName, "Order Ready For Pickup");
                        }
                        #endregion
                    }
                }
            }

            #endregion

            #region CONFIRMED ORDER BY RIDER  - SEND NOTIFICATION TO RESTAURANTS
            if (status == OrderHistoryEnu.ConfirmedByRider.Value.ToLower())
            {
                using (var dbContext = new DeliversEntities())
                {
                    var order = dbContext.Orders.FirstOrDefault(o => o.Id == orderId);
                    if (order != null)
                    {
                        var combinedOrders = dbContext.Orders.Where(o => o.SerialNo == order.SerialNo).ToList();
                        foreach (var dbo in combinedOrders)
                        {
                            var restKey = dbo.OrderDetails.FirstOrDefault().RestId;
                            var mapping = dbContext.User_Rest_Map.FirstOrDefault(m => m.RestId == restKey);
                            if (mapping != null)
                            {
                                var token = dbContext.Tokens.FirstOrDefault(u => u.UserId == mapping.UserId);
                                if (token != null)
                                {
                                    SendNotiInner(2, token.Token1, "Order no. " + orderId + " has been CONFIRMED by Rider", "Order confirmed by Rider");
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region CANCEL ORDER BY RIDER  - SEND NOTIFICATION TO RESTAURANTS
            if (status == OrderHistoryEnu.CanceledByRider.Value.ToLower())
            {
                using (var dbContext = new DeliversEntities())
                {
                    var order = dbContext.Orders.FirstOrDefault(o => o.Id == orderId);
                    if (order != null)
                    {
                        var combinedOrders = dbContext.Orders.Where(o => o.SerialNo == order.SerialNo).ToList();
                        foreach (var dbo in combinedOrders)
                        {
                            var restKey = dbo.OrderDetails.FirstOrDefault().RestId;
                            var mapping = dbContext.User_Rest_Map.FirstOrDefault(m => m.RestId == restKey);
                            if (mapping != null)
                            {
                                var token = dbContext.Tokens.FirstOrDefault(u => u.UserId == mapping.UserId);
                                if (token != null)
                                {
                                    SendNotiInner(2, token.Token1, "Order no. " + orderId + " has been canceled by Rider", "Order canceled by Rider");
                                }
                            }
                        }                       
                    }
                }
            }
            #endregion

            #region PICKED UP - SEND NOTIFICATION TO CUSTOMER 

            if (status == OrderHistoryEnu.WaitingForPickup.Value.ToLower())
            {
                using (var dbContext = new DeliversEntities())
                {
                    var order = dbContext.Orders.FirstOrDefault(o => o.Id == orderId);
                    if (order != null)
                    {
                        var restName = order.OrderDetails.FirstOrDefault().ItemDetail.ListItem.Name;

                        #region CUSTOMER
                        var token = dbContext.Tokens.FirstOrDefault(u => u.UserId == order.OrderBy);
                        if (token != null)
                        {
                            SendNotiInner(1, token.Token1, "Your order has been picked up from " + restName + " ", "Order Shipped");
                        }
                        #endregion
                    }
                }
            }

            #endregion

            #region DELIVERED - SEND NOTIFICATION TO RESTAURANT

            if (status == OrderHistoryEnu.CanceledByCustomer.Value.ToLower())
            {
                using (var dbContext = new DeliversEntities())
                {
                    var order = dbContext.Orders.FirstOrDefault(o => o.Id == orderId);
                    if (order != null)
                    {
                        var restId = order.OrderDetails.FirstOrDefault().RestId;
                        var mapping = dbContext.User_Rest_Map.FirstOrDefault(m => m.RestId == restId);
                        if (mapping != null)
                        {
                            var token = dbContext.Tokens.FirstOrDefault(u => u.UserId == mapping.UserId);
                            if (token != null)
                            {
                                SendNotiInner(2, token.Token1, "Order no. " + orderId + " has been delivered to customer", "Order delivered");
                            }
                        }
                    }
                }
            }

            #endregion

            return true;
        }

    }
}
