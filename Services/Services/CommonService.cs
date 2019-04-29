using Services.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Services
{
    public static class CommonService
    {
        public static DbGeography ConvertLatLonToDbGeography(string longitude, string latitude)
        {
            var point = string.Format("POINT({1} {0})", latitude, longitude);
            return DbGeography.FromText(point);
        }

        private static double Radians(double degrees)
        {
            return (0.017453292519943295 * degrees);
        }

        public static double? GetDistance(double lat1, double lon1, double lat2, double lon2)
        {
            double R = 6371; // km

            double sLat1 = Math.Sin(Radians(lat1));
            double sLat2 = Math.Sin(Radians(lat2));
            double cLat1 = Math.Cos(Radians(lat1));
            double cLat2 = Math.Cos(Radians(lat2));
            double cLon = Math.Cos(Radians(lon1) - Radians(lon2));

            double cosD = sLat1 * sLat2 + cLat1 * cLat2 * cLon;

            double d = Math.Acos(cosD);

            double dist = R * d;

            return dist;
        }

        public static bool VerifyOrderStatus (string orderStatus)
        {
            orderStatus = orderStatus.ToLower();
            if (
                orderStatus != OrderHistoryEnu.CanceledByCustomer.Value.ToLower() && 
                orderStatus != OrderHistoryEnu.CanceledByRestaurant.Value.ToLower() && 
                orderStatus != OrderHistoryEnu.Placed.Value.ToLower() &&
                orderStatus != OrderHistoryEnu.ConfirmedByCustomer.Value.ToLower() && 
                orderStatus != OrderHistoryEnu.ConfirmedByRestaurant.Value.ToLower() &&
               orderStatus != OrderHistoryEnu.WaitingForPickup.Value.ToLower() &&
               orderStatus != OrderHistoryEnu.PickedUp.Value.ToLower() && 
               orderStatus != OrderHistoryEnu.Deliverd.Value.ToLower() &&
               orderStatus != OrderHistoryEnu.ConfirmedByRider.Value.ToLower())
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static string GetUserTypeString(int? type)
        {
            switch (type)
            {
                case 0:
                    return "Rider";
                case 1:
                    return "Customer";
                case 2:
                    return "Owner";

                default:
                    return "NA";
            }
        }
    }
}
