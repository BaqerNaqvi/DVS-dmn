using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models
{
    public class GetOrderBySerialNoResponse
    {
        public int DeliveryTime { get; set; }

        public int DeliveryFee { get; set; }

        public List<OrderLocal> Orders { get; set; }
    }
}
