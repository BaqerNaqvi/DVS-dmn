using Services.DbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models
{
    public class OrderHistoryLocal
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public string Status { get; set; }

        public System.DateTime DateTime { get; set; }

        public OrderLocal Order { get; set; }

        public bool IsCurrent { get; set; }

    }


    public static class hismappppper {

        public static OrderHistoryLocal MapOrderHistory(this OrderHistory source)
        {
            return new OrderHistoryLocal
            {
                Id = source.Id,
                OrderId = source.OrderId,
                Status= source.Status,
                DateTime= source.DateTime,
                IsCurrent= source.IsCurrent
            };
        }

    }

}
