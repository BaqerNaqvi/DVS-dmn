using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Delives.pk.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Services.Models;
using Services.Services;

namespace Delives.pk.Apis
{
    public class ListController : ApiController
    {
        [HttpPost]
        [Route("api/Listing/GetItems")]  // of restaurants, super stores etc
        public ResponseModel GetListOfItems(GetListRequestModel listModel)
        {
            var response = new ResponseModel
            {
                Success = false,
                Messages = new List<string>(),
                Data= listModel
            };
           // return response;

            if (listModel == null ||  string.IsNullOrEmpty(listModel.Cords))    // 1. food  2.grocery 
            {
                response.Data = listModel;
                response.Messages.Add("Mandatory data can not be empty");
            }
            //if (listModel.IsWeb && listModel.TypeList.Count==0)    // 1. food  2.grocery 
            //{
            //    response.Data = listModel;
            //    response.Messages.Add("TypeList parameter can not be empty with IsWeb=true");
            //}
            else if (listModel.Cords.Split('_').Length != 2)
            {
                response.Messages.Add("Invalid Cord format. Please specify in Lat_Lang .i.e. '32.202895_74.176716'");
                response.Data = listModel;
            }
            else if (listModel.CurrentPage<=0 || listModel.ItemsPerPage<=0)
            {
                response.Data = listModel;
                response.Messages.Add("Current page/ItemsPerPage should be greater than 0");
            }
            else
            {
                try
                {
                    var items = ListService.GetItemsForList(listModel);
                    response.Data = items;
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


        [HttpPost]
        [Route("api/Listing/GetMenu")]
        public ResponseModel_ForGetMenu GetMenu(GetMenuRequestModel listModel)
        {
            var response = new ResponseModel_ForGetMenu
            {
                Success = false,
                Messages = new List<string>()
            };
            if (listModel == null || listModel.ItemId == 0)    
            {
                response.Messages.Add("ItemId can not be empty");
            }
            else if (listModel.CurrentPage <= 0 || listModel.ItemsPerPage <= 0)
            {
                response.Messages.Add("Current page/ItemsPerPage should be greater than 0");
            }
            else
            {
                try
                {
                    var menuItems = ListService.GetMenuByListItemId(listModel);
                    var rest = RestuarantService.GetRestaurantById(listModel.ItemId);
                    response.Data = menuItems;
                    response.RestaurentInfo = rest;
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


        [HttpPost]
        [Route("api/Listing/GetCatogries")]
        public ResponseModel_GetCatogries GetCatogries(GetCategoriesRequestModel model)
        {
            var response = new ResponseModel_GetCatogries
            {
                Success = false,
                Messages = new List<string>()
            };
            if (model==null)
            {
                response.Messages.Add("model is not mapped");
                response.Data = model;
            }
            else
            {
                try
                {
                    var menuItems = ListService.GetCategories(model.Status);
                    response.Data = menuItems;
                    response.MessageContents= "Enjoy free delivery for the whole week from 10am-7pm";
                    response.ShowMessage = true;
                    response.MessageTitle = "Free Delivery";

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

        [HttpPost]
        [Route("api/Item/AddReview")]
        public ResponseModel AddReview(RatingLocal rating)
        {
            var response = new ResponseModel
            {
                Success = false,
                Messages = new List<string>()
            };
            if (rating == null || string.IsNullOrEmpty(rating.RatedByUserId) ||
                rating.RatedToItem ==0)
            {
                response.Messages.Add("Data values are missing");
            }
            else if ( string.IsNullOrEmpty(rating.Comments) || rating.RatingStar < 0)
            {
                response.Messages.Add("Rating and comments can not be empty");
            }
            else
            {
                try
                {
                    var localUsermanager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
                    ApplicationUser user = localUsermanager.FindById(rating.RatedByUserId);
                    if (user == null)
                    {
                        return new ResponseModel
                        {
                            Success = false,
                            Messages = new List<string> { "User not found with given user id" },
                            Data = rating
                        };
                    }


                    var responseRate = ReviewService.AddReview(rating);
                    response.Data = rating;
                    response.Messages.Add(responseRate.ToString());
                    response.Success = responseRate;
                }
                catch (Exception excep)
                {
                    response.Messages.Add("Something bad happened.");
                }
            }
            return response;
        }

        [HttpPost]
        [Route("api/Listing/GetFavouriteItems")]  
        public ResponseModel GetListOfFavouriteItems(GetFavouriteListRequestModel listModel)
        {
            var response = new ResponseModel
            {
                Success = false,
                Messages = new List<string>()
            };
            if (listModel == null || string.IsNullOrEmpty(listModel.UserId) || string.IsNullOrEmpty(listModel.Cords))    
            {
                response.Messages.Add("Mandatory data can not be empty");
                response.Data = listModel;
            }
            else if (listModel.CurrentPage <= 0 || listModel.ItemsPerPage <= 0)
            {
                response.Messages.Add("Current page/ItemsPerPage should be greater than 0");
                response.Data = listModel;
            }
            else
            {
                try
                {
                    var items = ListService.GetI_Favourite_temsForList(listModel);
                    response.Data = items;
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

        [HttpPost]
        [Route("api/Listing/SetFavouriteItem")]
        public ResponseModel SetFavouriteItem(AddFavtItemRequestModel item)
        {
            var response = new ResponseModel
            {
                Success = false,
                Messages = new List<string>()
            };
            if (item == null || string.IsNullOrEmpty(item.UserId) || string.IsNullOrEmpty(item.ItemId))
            {
                response.Messages.Add("Mandatory data can not be empty");
            }
            else
            {
                try
                {
                    var responseCall = ListService.AddRemove_Favourite_Restaurent(item);
                    response.Data = item ;
                    response.Messages.Add(responseCall);
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
