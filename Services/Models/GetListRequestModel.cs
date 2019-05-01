using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models
{
    public class GetListRequestModel : PaggingModel
    {
        public int Type  { get; set; }   // being used for mobile 

        public List<int> TypeList { get; set; }   // being used for web and web filter , being used with IsWeb
        public bool IsWeb { get; set; }


        public string Cords { get; set; } // lat_long

        public string SearchTerm { get; set; }

       public string DistanceFrom { get; set; }
        public string DistanceTo { get; set; }
        public string Rating { get; set; }

        public string OrderBy { get; set; }
        public string SortOrder { get; set; }
    }


    public class GetFavouriteListRequestModel : PaggingModel
    {
        public string UserId { get; set; }
        public string Cords { get; set; } // lat_long
        public string SearchTerm { get; set; }

    }


    public class GetMenuRequestModel : PaggingModel
    {
        public long ItemId { get; set; }

        public string SearchTerm { get; set; }

    }

    public class GetItemDetailsRequestModel 
    {
        public long ItemId { get; set; }

    }

    public class GetCategoriesRequestModel
    {
        public bool Status { get; set; }

    }



    public class PlaceOrderRequestModel
    {
        public List<PlaceOrderItem> Items  { get; set; }

        public string Address { get; set; }

        public string Instructions { get; set; }

        public int PaymentMethod { get; set; }

        public string Cords { get; set; }

        public string OrderPlacedById { get; set; }  // for web only
        public bool FromWeb { get; set; }  // for web only
    }


    public class GetOrderDetailsRequest
    {
        public string OrderId { get; set; }

    }


    public class GetOrderDetailsBySerialNoRequest
    {
        public string SerialNo { get; set; }

    }

    public class PlaceOrderItem
    {
        public long ItemId { get; set; }

        public long RestId { get; set; }   // only for serer use

        public int Quantity { get; set; }
    }


    public class GetOrdersListRequestModel : PaggingModel
    {
        public string Cords { get; set; } // lat_long

        public string SearchTerm { get; set; }

    }
    public class NotificationRegisModel
    {
        public long AppId { get; set; }

        public string UserId { get; set; }

        public string Token { get; set; }

    }

    public class NotificationSendModel
    {
        public long AppId { get; set; }

        public string Text { get; set; }

        public List<string> UserIds { get; set; }

        public NotificationEnum Action { get; set; }
    }


    public class NotificationGetModel : PaggingModel
    {
        public long AppId { get; set; }

        public string UserId { get; set; }

    }



    public class SendNotiResponseModel
    {
        public long AppId { get; set; }

        public string UserId { get; set; }

        public List<string> Token { get; set; }

        public string Response { get; set; }

    }


    public class ChangeOrderStatusRequesrModel     {
        public string OrderId { get; set; } 

        public string NewStatus { get; set; }

        public string UserId { get; set; }

    }

    public class ApplyByRiderRequestmodel
    {
        public string OrderId { get; set; }
        public string UserId { get; set; }
    }

    public class CancelOrderByRiderRequestmodel
    {
        public string OrderId { get; set; }
        public string UserId { get; set; }
    }
    public class GetRestaurantOrdersForSpecificStatus  : PaggingModel
    {
        public string RestaurantId { get; set; }

        public string Status { get; set; }


    }

    public class GetOrdersForStatusRequestModel : PaggingModel
    {

        public string Status { get; set; }

    }

    public class GetRestaurantOrdersForSpecificDate : PaggingModel
    {
        public string RestaurantId { get; set; }

        public string StartDate { get; set; }

        public string EndDate { get; set; }

    }


    public class GeOrdersByDate_RiderModel : PaggingModel
    {
        public string UserId { get; set; }

        public string Status { get; set; }

        public string StartDate { get; set; }

        public string EndDate { get; set; }

    }


    public class OrderHistoryForDBoyRequesrModel
    {
        public string UserId { get; set; }

        public string Status { get; set; }

    }

    public class userLocationModel
    {
        public string UserId { get; set; }

        public string Cords { get; set; }

    }

    public class CustomerHistoryResponseModel
    {
        public string CustomerName { get; set; }

        public List<OrderLocal> Orders { get; set; }

    }

    public class DboyInfoRequesrModel
    {
        public string UserId { get; set; }

    }

    public class OrderHistoryForCustomerRequesrModel
    {
        public string UserId { get; set; }

    }

    public class UserIssueRequestModel
    {
        public string UserId { get; set; }

        public string Text { get; set; }

    }

  

    public class AddFavtItemRequestModel
    {
        public string UserId { get; set; }

        public string ItemId { get; set; }
         
        public bool IsFavourite { get; set; }

    }

}
