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
    public class NotificationController : ApiController
    {
        [HttpPost]
        [Route("api/Notification/Registration")]
        public ResponseModel NotiRegistration(NotificationRegisModel model)
        {
            var response = new ResponseModel
            {
                Success = false,
                Messages = new List<string>()
            };
            if (model == null || string.IsNullOrEmpty(model.UserId) || string.IsNullOrEmpty(model.Token))
            {
                response.Messages.Add("Mandatory data can not be empty");
                response.Data = model;
            }           
            else
            {
                try
                {
                    var res = NotificationService.AddNewToken(model);
                    response.Data = model;
                    response.Messages.Add("Token added");
                    response.Success = res;
                }
                catch (Exception excep)
                {
                    response.Messages.Add("Something bad happened.");
                }
            }
            return response;
        }


        [HttpPost]
        [Route("api/Notification/Send")]
        public ResponseModel SendNoti(NotificationSendModel model)
        {
            var response = new ResponseModel
            {
                Success = false,
                Messages = new List<string>()
            };
            if (model == null || model.AppId==0)
            {
                response.Messages.Add("Mandatory data can not be empty. AppId can not be 0.");
                response.Data = model;
            }
            // 
            else if (model.Action!= NotificationEnum.testing || (model.UserIds == null || model.UserIds.Count == 0) || string.IsNullOrEmpty(model.Text))
            {
                response.Messages.Add("You need to add UserIds for notifications with proper Action & Text ");
                response.Data = model;
            }
            else
            {
                try
                {
                    var res = NotificationService.SendNotification(model);
                    response.Data = res;
                    response.Messages.Add("SUCCESS");
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
        [Route("api/Notification/Get")]
        public ResponseModel GetNoti(NotificationGetModel model)
        {
            var response = new ResponseModel
            {
                Success = false,
                Messages = new List<string>()
            };
            if (model == null || model.AppId==0 ||
               string.IsNullOrEmpty(model.UserId))
            {
                response.Messages.Add("Mandatory data can not be empty");
                response.Data = model;
            }
            else if (model.CurrentPage <= 0 || model.ItemsPerPage <= 0)
            {
                response.Messages.Add("Current page/ItemsPerPage should be greater than 0");
            }
            else
            {
                try
                {
                    var notis = NotificationService.GetAllNotifications(model);
                    response.Data = notis;
                    response.Messages.Add("SUCCESS");
                    response.Success = true;
                }
                catch (Exception excep)
                {
                    response.Messages.Add("Something bad happened.");
                }
            }
            return response;
        }

    }
}
