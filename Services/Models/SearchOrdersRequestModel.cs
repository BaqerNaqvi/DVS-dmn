using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models
{
    public class SearchOrdersRequestModel
    {
        public List<string> ListStatuses { get; set; }
        public List<long?> ListRestaurants { get; set; }
        public List<string> ListRiders { get; set; }
    }

    public class SearchOrdersRequestModelSingle
    {
        public string Status { get; set; }
        public long? Restaurant { get; set; }
        public string Rider { get; set; }
        public DateTime? OrderDateFrom { get; set; }
        public DateTime? OrderDateTo { get; set; }
    }
}
