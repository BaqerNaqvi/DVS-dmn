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
    [Authorize]
    public class AdminController : ApiController
    {
        [HttpPost]
        [Route("api/admin/addIssue")]
        public ResponseModel addIssue(UserIssueRequestModel listModel)
        {
            var response = new ResponseModel
            {
                Success = false,
                Messages = new List<string>()
            };
            if (listModel == null ||  string.IsNullOrEmpty(listModel.UserId) || string.IsNullOrEmpty(listModel.Text) )    
            {
                response.Messages.Add("Data is not mapped");
            }
            else
            {
                try
                {
                    var item = AdminService.AddUserIssue(listModel);
                    response.Data = "Thank you.Your request # is : "+Guid.NewGuid().ToString().Substring(0,4)+". We will get back to you soon.";
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


    }
}