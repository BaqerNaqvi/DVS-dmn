using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models
{
    public class AllRiders : GenericResponse
    {
        public List<UserLocal> ListRiders { get; set; }
    }
}
