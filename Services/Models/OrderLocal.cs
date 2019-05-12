using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.DbContext;
using System.ComponentModel.DataAnnotations;

namespace Services.Models
{
    public class OrderLocal
    {
        [Display(Name = "Order Id")]
        public long Id { get; set; }
        public string DateTime { get; set; }

        [Display(Name = "Current Status")]
        public string Status { get; set; }

        [Display(Name = "Delivery Address")]
        public string Address { get; set; }

        [Display(Name = "Order By Id")]
        public string OrderBy { get; set; }
        public int Amount { get; set; }
        public string EstimatedTime { get; set; }
        public string Instructions { get; set; }

        [Display(Name = "Order By")]
        public string OrderByName { get; set; }
        public string OrderByNumber { get; set; }
        public  List<OrderDetailLocal> OrderDetails { get; set; }

        public List<OrderHistoryLocal> History { get; set; }

        public string SerialNo { get; set; }
        public string PickedBy { get; set; }
        public int? DeliveryCost { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string RestName { get; set; }

    }

    public class OrderLocal_waitingForPickup
    {
        public long Id { get; set; }
        public string DateTime { get; set; }
        public string[] PickupAddresses { get; set; }

        public string DeliveryAddress { get; set; }

        public int Amount { get; set; }

        public string Instructions { get; set; }

        public string OrderByName { get; set; }

        public List<OrderHistoryLocal> History { get; set; }
        public int? DeliveryCost { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string RestName { get; set; }

    }


    public static class OrderMapper
    {
        public static OrderLocal MappOrder(this Order source)
        {
            return new OrderLocal
            {
                Id = source.Id,
                DateTime = source.DateTime.ToShortDateString() +" "+source.DateTime.ToShortTimeString(),
                Address = source.Address,
                Amount = source.Amount,
                EstimatedTime = source.EstimatedTime,
                Instructions = source.Instructions,
                Status = source.Status,
                OrderBy = source.OrderBy,
                OrderByName = source.AspNetUser.FirstName +" "+ source.AspNetUser.LastName,
                OrderByNumber= source.AspNetUser.UserName,
                OrderDetails = source.OrderDetails.Select( det => det.MapODetailLocal()).ToList(),
                SerialNo = source.SerialNo,
                PickedBy=  source.PickedBy,
                History= source.OrderHistories.Select(his => his.MapOrderHistory()).ToList(),
                DeliveryCost= source.DeliveryCost,
                UpdatedAt= (DateTime)source.UpdatedAt,
                RestName= source.OrderDetails.FirstOrDefault().ItemDetail.ListItem.Name
            };
        }

        public static OrderLocal_waitingForPickup MappOrderWaitingForPickup(this Order source)
        {
            return new OrderLocal_waitingForPickup
            {
                Id = source.Id,
                DateTime = source.DateTime.ToShortDateString() + " " + source.DateTime.ToShortTimeString(),
                DeliveryAddress = source.Address,
                Amount = source.Amount,
                Instructions = source.Instructions,
                OrderByName = source.AspNetUser.FirstName + " " + source.AspNetUser.LastName,
                PickupAddresses = new []{source.OrderDetails.FirstOrDefault().ItemDetail.ListItem.Address},
                History = source.OrderHistories.Select(his => his.MapOrderHistory()).ToList(),
                DeliveryCost = source.DeliveryCost,
                UpdatedAt = (DateTime)source.UpdatedAt,
                RestName = source.OrderDetails.FirstOrDefault().ItemDetail.ListItem.Name



            };
        }
    }

    public class EditOrderModel 
    {
        public OrderLocal Order { get; set; }
        public List<UserLocal> Riders { get; set; }
        public List<OrderHistoryEnu> OrderStatus { get; set; }
    }

}
