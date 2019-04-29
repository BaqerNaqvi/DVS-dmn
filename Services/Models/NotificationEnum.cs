using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models
{
    public enum NotificationEnum
    {
        placed=1,
        canceled=2,
        confirmed=3,
        waitingForPickup=4,
        pickedUp=5,
        deliverd=6,
        testing=7
    }
}
