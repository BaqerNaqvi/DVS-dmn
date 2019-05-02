using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Delives.pk.Models
{
    public class ResponseModel
    {
        public bool Success { get; set; }

        public List<string> Messages { get; set; }

        public Object Data { get; set; }
    }


    public class ResponseModel_ForGetMenu : ResponseModel
    {
        public Object RestaurentInfo { get; set; }
    }

    public class ResponseModel_Login : ResponseModel
    {
        public Object Code { get; set; }
    }


    public class ResponseModel_PlaceOrder : ResponseModel
    {
        public Object OrderId { get; set; }
        public Object SerialNo { get; set; }
    }

    public class ResponseModel_GetCatogries : ResponseModel
    {
        public bool ShowMessage { get; set; }
        public string MessageTitle { get; set; }
        public string MessageContents { get;set;}
    }
}