using Delives.pk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;

namespace Delives.pk.Apis
{
    public class EchoController : ApiController
    {            
        [System.Web.Mvc.HttpGet]
        [System.Web.Mvc.Route("api/Echo")]
        public IHttpActionResult Get()
        {
            return Ok("Happy Documentation!");
        }
    }
}
