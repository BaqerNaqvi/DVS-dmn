using Services.DbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models
{
    public class GetListResponseModel: PaggingModel
    {
       public List<ListItemLocal> Items { get; set; }
       
    }

    public class AboutUsResponseModel
    {
        public List<string> Items { get; set; }

    }


    public class FaqModel
    {
        public string Question { get; set; }

        public string Answer { get; set; }

    }


    public class GetFaqResponseModel
    {
        public List<FaqModel> Faqs { get; set; }

    }


    public class GetTermsResponseModel
    {
        public List<string> Items { get; set; }

    }

    public class ContactUsResponseModel
    {
        public string Phone { get; set; }
        public string Mobile { get; set; }

        public string Address { get; set; }

        public string Email { get; set; }
    }

    public class GetAllNotificationResponseModel : PaggingModel
    {
        public List<Notification> Notifications { get; set; }

    }

    public class GetOrdersResponseModel : PaggingModel
    {
        public List<OrderLocal_waitingForPickup> Orders { get; set; }
    }

    public class GetOrdersForStatus : PaggingModel
    {
        public List<OrderLocal> Orders { get; set; }
    }

    public class DeliveredOrdersDBoyResponseModel
    {
        public List<OrderLocal> Orders { get; set; }

    }
    public class PlaceOrderResponseModel
    {
        public string EstimatedTime { get; set; }

        public string SerailNo { get; set; }
        public List<string> OrderIds { get; set; }

    }

    public class ApplyRiderResponse
    {
        public bool isSuccesss { get; set; }
        public string Message { get; set; }
    }

    public class OrdersForRestaurantResponseModel : PaggingModel
    {
        public List<OrderLocal> Orders { get; set; }

    }


    public class OrdersForRidersResponseModel : PaggingModel
    {
        public List<OrderLocal> Orders { get; set; }

    }
}
