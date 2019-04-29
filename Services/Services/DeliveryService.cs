using Services.DbContext;
using Services.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Services
{
    public static class DeliveryService
    {
        public static List<UserLocal> GetAvaialableDeliveryBoys(DbGeography location)
        {

            DbGeography userLoc = location;
            #region location conversion
            //List<string> latlng = new List<string>();
            //if (!string.IsNullOrEmpty(location) && location != "")
            //{
            //    latlng = location.Split('_').ToList();
            //    if (latlng.Count == 2)
            //    {
            //        userLoc = CommonService.ConvertLatLonToDbGeography(latlng[1], latlng[0]); // lat _ lng
            //    }
            //} 
            #endregion

            var riders = new List<UserLocal>();

            using (var dbContext = new DeliversEntities())
            {
                var allDevliveryBoyes = dbContext.AspNetUsers.Where(o => o.Type == 0 && (o.Status ?? false) && (o.IsApproved ?? false)).ToList();

                if(allDevliveryBoyes!=null && allDevliveryBoyes.Any())
                {
                    foreach(var d in allDevliveryBoyes)
                    {
                        var dloc = dbContext.Rider_Location_Map.FirstOrDefault(loc => loc.UserId == d.Id);
                        if (dloc != null)
                        {

                            var dist = CommonService.GetDistance((double)userLoc.Latitude, (double)userLoc.Longitude, Convert.ToDouble(dloc.Location.Latitude), Convert.ToDouble(dloc.Location.Longitude));
                          //  if ((int)dist < Convert.ToInt16(10))
                            {
                                riders.Add(d.Mapper());
                            }
                        }
                    }
                }
            }
            return riders;
        }
    }
}
