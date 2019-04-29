using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Delives.pk.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Services.DbContext;
using static Delives.pk.Models.UserMapper;

namespace Delives.pk.Security
{
    public class AuthHandler : DelegatingHandler
    {
        string _userName = "";
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {

            //IEnumerable<string> headerValues = request.Headers.GetValues("MyAxe");
            //var id = headerValues.FirstOrDefault();

            if (ValidateCredentials(request.Headers.Authorization))
            {
                Thread.CurrentPrincipal = new DeliversAPIPrincipal(_userName);
                HttpContext.Current.User = new DeliversAPIPrincipal(_userName);
            }           
            var response = await base.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                response.Headers.Add("WwwAuthenticate", "Basic");
                response.StatusCode= HttpStatusCode.BadRequest;
                return response;
            }
            return response;            
        }

        //Method to validate credentials from Authorization
        //header value
        private bool ValidateCredentials(AuthenticationHeaderValue authenticationHeaderVal)
        {
            try
            {
                if (authenticationHeaderVal != null && !String.IsNullOrEmpty(authenticationHeaderVal.Parameter))
                {
                    string[] decodedCredentials
                    = Encoding.ASCII.GetString(Convert.FromBase64String(
                    authenticationHeaderVal.Parameter))
                    .Split(new[] { ':' });
                  
                    var context = new ApplicationDbContext();
                    var userStore = new UserStore<ApplicationUser>(context);
                    var userManager = new UserManager<ApplicationUser>(userStore);
                    var user = userManager.Find(decodedCredentials[0], decodedCredentials[1]);
                    _userName = decodedCredentials[0];
                    return user!=null;

                }
                return false;//request not authenticated.
            }
            catch
            {
                return false;
            }
        }
    }
}