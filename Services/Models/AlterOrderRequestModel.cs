using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models
{
    public class AlterOrderRequestModel
    {
        public long OrderId { get; set; }

        public string Comments { get; set; }

        public List<subItemClass> Items { get; set; }
    }

    public class subItemClass
    {
        public long itemId { get; set; }
        public int quantity { get; set; }

    }
}
