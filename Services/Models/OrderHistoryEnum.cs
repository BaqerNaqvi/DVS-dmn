using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models
{
   
    public class OrderHistoryEnu
    {
        private OrderHistoryEnu(string value, int order) {
            Value = value;
            Order = order;
        }

        public string Value { get; set; }
        public int Order { get; set; }

        public static OrderHistoryEnu Placed { get { return new OrderHistoryEnu("Placed",0); } }
        public static OrderHistoryEnu CanceledByCustomer { get { return new OrderHistoryEnu("CanceledByCustomer", 1); } }
        public static OrderHistoryEnu CanceledByRestaurant { get { return new OrderHistoryEnu("CanceledByRestaurant", 1); } }

        public static OrderHistoryEnu ConfirmedByCustomer { get { return new OrderHistoryEnu("ConfirmedByCustomer", 1); } }
        public static OrderHistoryEnu ConfirmedByRestaurant { get { return new OrderHistoryEnu("ConfirmedByRestaurant", 1); } }

        public static OrderHistoryEnu ConfirmedByRider { get { return new OrderHistoryEnu("ConfirmedByRider", 2); } }

        public static OrderHistoryEnu WaitingForPickup { get { return new OrderHistoryEnu("WaitingForPickup",2); } }

        public static OrderHistoryEnu CanceledByRider { get { return new OrderHistoryEnu("CanceledByRider", 3); } }

        public static OrderHistoryEnu PickedUp { get { return new OrderHistoryEnu("PickedUp",3); } }

        public static OrderHistoryEnu Deliverd { get { return new OrderHistoryEnu("Delivered",4); } }

        public static OrderHistoryEnu GetOrderStatus(string stat)
        {
            stat = stat.ToLower();
            if (stat == "placed")
            {
                return new OrderHistoryEnu("Placed", 0);
            }
            else if (stat == "canceledbycustomer")
            {
                return new OrderHistoryEnu("CanceledByCustomer", 1);
            }
            else if (stat == "canceledbyrestaurant")
            {
                return new OrderHistoryEnu("CanceledByRestaurant", 1);
            }
            else if (stat == "confirmedbycustomer")
            {
                return new OrderHistoryEnu("ConfirmedByCustomer", 1);
            }
            else if (stat == "confirmedbyrestaurant")
            {
                return new OrderHistoryEnu("ConfirmedByRestaurant", 1);
            }
            else if (stat == "confirmedbyrider")
            {
                return new OrderHistoryEnu("ConfirmedByRider", 2);
            }
            else if (stat == "waitingforpickup")
            {
                return new OrderHistoryEnu("WaitingForPickup", 2);
            }
            else if (stat == "canceledbyrider")
            {
                return new OrderHistoryEnu("CanceledByRider", 3);
            }
            else if (stat == "pickedup")
            {
                return new OrderHistoryEnu("PickedUp", 3);
            }
            else if (stat == "delivered")
            {
                return new OrderHistoryEnu("Delivered", 4);
            }
            else
                return null;
        }
    }
}
