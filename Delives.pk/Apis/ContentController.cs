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
    public class ContentController : ApiController
    {
        [HttpPost]
        [Route("api/Content/aboutus")]
        public ResponseModel GetAboutUs()
        {
            var response = new ResponseModel
            {
                Success = false,
                Messages = new List<string>()
            };            
                try
                {
                    //var item = ItemDetailsService.GetItemDetailLocalById(listModel.ItemId);
                    response.Data = new AboutUsResponseModel { Items = new List<string> {"This is about us for delivers.pk service and we will add multiple contents here from the server.","What do you say?"} };
                    response.Messages.Add("Success");
                    response.Success = true;
                }
                catch (Exception excep)
                {
                    response.Messages.Add("Something bad happened.");
                }
            
            return response;
        }

        [HttpPost]
        [Route("api/Content/contactus")]
        public ResponseModel GetContactUs()
        {
            var response = new ResponseModel
            {
                Success = false,
                Messages = new List<string>()
            };
            try
            {
                //var item = ItemDetailsService.GetItemDetailLocalById(listModel.ItemId);
                response.Data = new ContactUsResponseModel
                {
                    Address = "292 F-Block Johar Town Lahore",
                    Email = "contact@delivers.pk",
                    Mobile = "923466043805",
                    Phone = "92553896536"
                };
                response.Messages.Add("Success");
                response.Success = true;
            }
            catch (Exception excep)
            {
                response.Messages.Add("Something bad happened.");
            }

            return response;
        }

        [HttpPost]
        [Route("api/Content/faqs")]
        public ResponseModel GetFaqs()
        {
            var response = new ResponseModel
            {
                Success = false,
                Messages = new List<string>()
            };
            try
            {
                //var item = ItemDetailsService.GetItemDetailLocalById(listModel.ItemId);
                response.Data = new GetFaqResponseModel {
                    Faqs = new List<FaqModel> {
                        new FaqModel {Question="How can I rider for delivers.pk?",Answer="Just call us on our helpline we will guide you through." },
                        new FaqModel {Question="This is second question added from the server of delivers.pk and it is valid?", Answer="Yes i guess it is working for now and i am happy that we are going to launch it very soon in this market" }
                    }
                };
                response.Messages.Add("Success");
                response.Success = true;
            }
            catch (Exception excep)
            {
                response.Messages.Add("Something bad happened.");
            }

            return response;
        }

        [HttpPost]
        [Route("api/Content/terms")]
        public ResponseModel GetTerms()
        {
            var response = new ResponseModel
            {
                Success = false,
                Messages = new List<string>()
            };
            try
            {
                //var item = ItemDetailsService.GetItemDetailLocalById(listModel.ItemId);
                response.Data = new GetTermsResponseModel
                {
                    Items = new List<string> { "This is term added from the server of delivers.pk and it looks very nice and clean",
                    "This is new and i am not gonna like it you know , right?"
                }
                };
                response.Messages.Add("Success");
                response.Success = true;
            }
            catch (Exception excep)
            {
                response.Messages.Add("Something bad happened.");
            }

            return response;
        }


    }
}
