using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Delives.pk.Models;
using Services.Models;
using Services.Services;

namespace Delives.pk.Apis
{
    [Authorize]
    public class ItemController : ApiController
    {
        [HttpPost]
        [Route("api/Item/GetDetail")]
        public ResponseModel GetDetail(GetItemDetailsRequestModel listModel)
        {
            var response = new ResponseModel
            {
                Success = false,
                Messages = new List<string>()
            };
            if (listModel == null || listModel.ItemId == 0)    // 1. food  2.grocery 
            {
                response.Messages.Add("ItemId can not be empty");
            }            
            else
            {
                try
                {
                    var item = ItemDetailsService.GetItemDetailLocalById(listModel.ItemId);
                    response.Data = item;
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
