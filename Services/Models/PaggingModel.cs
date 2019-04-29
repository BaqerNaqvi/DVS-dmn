using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models
{
    public class PaggingModel
    {
        public int TotalItems { get; set; }

        public int CurrentPage { get; set; }


        public int ItemsPerPage { get; set; }
    }
}
