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
    public class MobileController : ApiController
    {


        [HttpPost]
        [Route("api/admin/GetContents")]
        public MobileContentsClass GetContents(UserIssueRequestModel listModel)
        {
            return AdminService.GetMobileContents();
        }

        
    }
}
