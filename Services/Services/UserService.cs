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
    public static class UserService
    {
        public static bool UpdateUserLocation(userLocationModel requestModel)
        {
            DbGeography userLoc = null;
            List<string> latlng = new List<string>();
            if (!string.IsNullOrEmpty(requestModel.Cords) && requestModel.Cords != "")
            {
                latlng = requestModel.Cords.Split('_').ToList();
                if (latlng.Count == 2)
                {
                    userLoc = CommonService.ConvertLatLonToDbGeography(latlng[1], latlng[0]); // lat _ lng
                    using (var dbContext = new DeliversEntities())
                    {
                        var userlocmap = dbContext.Rider_Location_Map.FirstOrDefault(l => l.UserId == requestModel.UserId);
                        if (userlocmap == null)
                        {
                            var obj = new Rider_Location_Map
                            {
                                UserId = requestModel.UserId,
                                Location= userLoc,
                                LastUpdated= CommonService.GetSystemTime()
                            };
                            dbContext.Rider_Location_Map.Add(obj);
                        }
                        else
                        {
                            userlocmap.Location = userLoc;
                            userlocmap.LastUpdated = DateTime.Now;
                        }
                        dbContext.SaveChanges();
                        return true;
                    }
                }
            }
            return false;
        }

        public static List<UserLocal> GetUsersByype(string type)
        {
            using (var dbContext = new DeliversEntities())
            {
                return dbContext.AspNetUsers.Where(u => u.Type.ToString() == type).ToList().Select(o => o.Mapper()).ToList();
            }
        }
    }
}
