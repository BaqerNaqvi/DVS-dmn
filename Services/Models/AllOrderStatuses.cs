using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models
{
    public class AllOrderStatuses : GenericResponse
    {
        public List<OrderHistoryEnu> ListStatuses {get; set; }
    }
}
