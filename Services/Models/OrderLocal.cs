using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.DbContext;

namespace Services.Models
{
    public class OrderLocal
    {
        public long Id { get; set; }
        public string DateTime { get; set; }
        public string Status { get; set; }
        public string Address { get; set; }
        public string OrderBy { get; set; }
        public int Amount { get; set; }
        public string EstimatedTime { get; set; }
        public string Instructions { get; set; }

        public string OrderByName { get; set; }
        public  List<OrderDetailLocal> OrderDetails { get; set; }

        public List<OrderHistoryLocal> History { get; set; }

        public string SerialNo { get; set; }
        public string PickedBy { get; set; }
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
                OrderDetails = source.OrderDetails.Select( det => det.MapODetailLocal()).ToList(),
                SerialNo = source.SerialNo,
                PickedBy=  source.PickedBy,
                History= source.OrderHistories.Select(his => his.MapOrderHistory()).ToList()
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
                History = source.OrderHistories.Select(his => his.MapOrderHistory()).ToList()

            };
        }
    }
}
