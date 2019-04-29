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
    public class UserApiController : ApiController
    {

        [HttpPost]
        [Route("api/User/UpdateLocation")]
        public ResponseModel UpdateUserLocation(userLocationModel listModel)
        {
            var response = new ResponseModel
            {
                Success = false,
                Messages = new List<string>()
            };
            if (listModel == null || string.IsNullOrEmpty(listModel.UserId) || string.IsNullOrEmpty(listModel.Cords))
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
                    response.Success = UserService.UpdateUserLocation(listModel);
                    response.Data = listModel;
                    response.Messages.Add(response.Success.ToString());
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
