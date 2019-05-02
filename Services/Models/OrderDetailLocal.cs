using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.DbContext;

namespace Services.Models
{
   public class OrderDetailLocal
    {
        public long Id { get; set; }
        public long ItemId { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public long OrderId { get; set; }
        public long RestId { get; set; }


        public virtual ItemDetailLocal ItemDetail { get; set; }
        public virtual OrderLocal Order { get; set; }
    }

    public static class ODetailMapper
    {
        public static OrderDetailLocal MapODetailLocal(this OrderDetail source)
        {
            return new OrderDetailLocal
            {
                Id = source.Id,
                ItemDetail = source.ItemDetail.ItemDetailMapper(),
              //  Order = source.Order.MappOrder(),
                ItemId = source.ItemId,
                ItemName= source.ItemDetail.Name,
                OrderId = source.OrderId,
                Quantity = source.Quantity,
                RestId= source.RestId
            };
        }
    }
}
