using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.DbContext;
using Services.Models;
using System.Data.Entity.Spatial;

namespace Services.Services
{
    public static class ListService
    {
        public static long Create(ListItemLocal source)
        {
            DbGeography loc = null;
            List<string> latlng = new List<string>();
            if (!string.IsNullOrEmpty(source.Cords) && source.Cords != "")
            {
                latlng = source.Cords.Split('_').ToList();
                if (latlng.Count == 2)
                {
                    loc = CommonService.ConvertLatLonToDbGeography(latlng[1], latlng[0]); // lat _ lng
                }
            }
            var obj = new ListItem
            {
                Address = source.Address,
                BgImage = "http://via.placeholder.com/200x100",
                Closes = source.Closes,
                Cords = loc,
                CreationDate = DateTime.Now, 
                Description = source.Description,
                LastEdit = DateTime.Now,
                Location = loc,
                Name = source.Name,
                Phone = source.Phone,
                Opens = source.Opens,
                Status = source.Status,
                Type = source.Type,
                Rating = "0.00",
                MinOrder = source.MinOrder,
                LogoImage = "http://via.placeholder.com/200x100"

            };
            using (var dbContext = new DeliversEntities())
            {
                dbContext.ListItems.Add(obj);
                dbContext.SaveChanges();
            }
            return obj.Id;
        }
        public static long Edit(ListItemLocal source)
        {
            DbGeography loc = null;
            List<string> latlng = new List<string>();
            if (!string.IsNullOrEmpty(source.Cords) && source.Cords != "")
            {
                latlng = source.Cords.Split('_').ToList();
                if (latlng.Count == 2)
                {
                    loc = CommonService.ConvertLatLonToDbGeography(latlng[1], latlng[0]); // lat _ lng
                }
            }
            using (var dbContext = new DeliversEntities())
            {
                var obj = dbContext.ListItems.FirstOrDefault(x => x.Id == source.Id);
                obj.Address = source.Address;
                obj.Closes = source.Closes;
                obj.Cords = loc;
                obj.Description = source.Description;
                obj.LastEdit = DateTime.Now;
                obj.Location = loc;
                obj.Name = source.Name;
                obj.Phone = source.Phone;
                obj.Opens = source.Opens;
                obj.Status = source.Status;
                obj.Type = source.Type;
                obj.MinOrder = source.MinOrder;
                if (!string.IsNullOrWhiteSpace(source.BgImage))
                    obj.BgImage = source.BgImage;
                if (!string.IsNullOrWhiteSpace(source.LogoImage))
                    obj.LogoImage = source.LogoImage;
                dbContext.SaveChanges();
            }
            return source.Id;
        }
        public static bool UpdateImages(ListItemLocal source)
        {
            using (var dbContext = new DeliversEntities())
            {
                var item = dbContext.ListItems.First(x => x.Id == source.Id);
                item.BgImage = source.BgImage;
                item.LogoImage = source.LogoImage;
                item.LastEdit = DateTime.Now;
                dbContext.SaveChanges();
            }
            return true;
        }
        public static async Task<ListItemLocal> DetailsAsync(long id)
        {
            ListItemLocal listItemLocal = null;
            using (var dbContext = new DeliversEntities())
            {
                ListItem listItem = await dbContext.ListItems.FindAsync(id);
                listItemLocal = listItem.MapListItem();
            }
            return listItemLocal;
        }

        public static GetListResponseModel GetItemsForList(GetListRequestModel requestModel)
        {
            var distanceFrom = 0;
            var distanceTo = 1000000;
            float rating = 0;

            if (!string.IsNullOrEmpty(requestModel.DistanceFrom))
            {
                distanceFrom = Convert.ToInt16(requestModel.DistanceFrom);
            }
            if (!string.IsNullOrEmpty(requestModel.DistanceTo))
            {
                distanceTo = Convert.ToInt16(requestModel.DistanceTo);
            }
            if (!string.IsNullOrEmpty(requestModel.Rating))
            {
                rating = (float)Convert.ToDouble(requestModel.Rating);
            }

            DbGeography userLoc = null;
            List<string> latlng = new List<string>();
            if (!string.IsNullOrEmpty(requestModel.Cords) && requestModel.Cords != "")
            {
                latlng = requestModel.Cords.Split('_').ToList();
                if (latlng.Count == 2)
                {
                    userLoc = CommonService.ConvertLatLonToDbGeography(latlng[1], latlng[0]); // lat _ lng
                }
            }
            using (var dbContext = new DeliversEntities())
            {
                var allCats = GetCategories(true);
                if (requestModel.IsWeb && (requestModel.TypeList == null || requestModel.TypeList.Count() == 0))
                {
                    requestModel.TypeList = new List<int>();
                    requestModel.TypeList = allCats.Select(c => (int)c.CatId).ToList();
                }
                requestModel.CurrentPage--;
                var response = new GetListResponseModel();
                var newList = new List<ListItemLocal>();

                string searchText = null;
                if (!string.IsNullOrEmpty(requestModel.SearchTerm))
                {
                    searchText = requestModel.SearchTerm.ToLower();
                }

                var list = dbContext.ListItems.Where(item => ((requestModel.IsWeb && requestModel.TypeList.Any(o => o == item.Type)) ||
                 (item.Type == requestModel.Type && !requestModel.IsWeb))

                 &&
                 (string.IsNullOrEmpty(searchText) ||
                 item.Name.ToLower().Contains(searchText) ||
                 item.Address.ToLower().Contains(searchText) ||
                 item.Description.ToLower().Contains(searchText) ||
                 (item.ItemDetails.Any(det => det.Name.ToLower().Contains(searchText)))
                 )).ToList();
                if (list.Any())
                {
                    var finals = list.Select(obj => obj.MapListItem()).ToList();
                    foreach (var rest in finals)
                    {
                        var restRate = (float)Convert.ToDouble(rest.Rating);
                        var dist = CommonService.GetDistance((double)userLoc.Latitude, (double)userLoc.Longitude, Convert.ToDouble(rest.LocationObj.Latitude), Convert.ToDouble(rest.LocationObj.Longitude));
                        if ((int)dist >= distanceFrom && (int)dist <= distanceTo && restRate >= rating)
                        {
                            var disst = Math.Round((double)dist, 2);
                            rest.LocationObj = null;
                            rest.Distance = disst;
                            rest.Name = rest.Name;
                            rest.TypeName = allCats.FirstOrDefault(c => c.CatId == rest.Type).Name;
                            newList.Add(rest);
                        }
                    }
                    newList = newList.OrderBy(obj => obj.Distance).ToList();
                    var take = newList.Skip(requestModel.CurrentPage * requestModel.ItemsPerPage).
                        Take(requestModel.ItemsPerPage).ToList();
                    response.Items = take;
                }
                requestModel.CurrentPage++;
                response.ItemsPerPage = requestModel.ItemsPerPage;
                response.CurrentPage = requestModel.CurrentPage;
                response.TotalItems = newList.Count;
                return response;
            }
        }



        public static List<ListItemLocal> GetItemsForList_AdminPanel()
        {

            using (var dbContext = new DeliversEntities())
            {
                return dbContext.ListItems.Where(L => L.Status).ToList().Select(o => o.MapListItem()).ToList();
            }
        }


        public static List<ItemDetailLocal_Short> GetMenuByListItemId(GetMenuRequestModel model)
        {
            model.CurrentPage--;
            using (var dbcontext = new DeliversEntities())
            {
                var items = dbcontext.ItemDetails.Where(det => det.ListItemId == model.ItemId
                && (string.IsNullOrEmpty(model.SearchTerm) || det.Name.ToLower().Contains(model.SearchTerm.ToLower())
                || det.Description.ToLower().Contains(model.SearchTerm.ToLower()))
                ).ToList();
                if (items.Any())
                {
                    var take = items.Skip(model.CurrentPage * model.ItemsPerPage).
                            Take(model.ItemsPerPage).ToList();

                    return take.Select(obj => obj.ItemDetailShortMapper()).ToList();
                }
                return new List<ItemDetailLocal_Short>();
            }
        }


        public static List<ListCategoryLocal> GetCategories(bool staus)
        {
            using (var dbcontext = new DeliversEntities())
            {
                var items = dbcontext.ListCategories.Where(det => det.Status == staus).ToList().Select(p => p.Mapper()).ToList();
                if (items.Any())
                {
                    foreach (var item in items)
                    {
                        item.ItemCount = dbcontext.ListItems.Count(o => o.Type == item.CatId);
                    }
                }
                return items;
            }
        }

        public static ListCategoryLocal GetRestaurantType(long id)
        {
            using (var dbcontext = new DeliversEntities())
            {
                var item = dbcontext.ListCategories.Where(det => det.Status == true && det.CatId == id).ToList().Select(p => p.Mapper()).FirstOrDefault();
                return item;
            }
        }
        public static void AddNewRestaurent(ListItemLocal source)
        {
            using (var dbContext = new DeliversEntities())
            {
                DbGeography loc = null;
                if (!String.IsNullOrEmpty(source.Location) && source.Location != "")
                {
                    var latlng = source.Location.Split('_');
                    if (latlng.Length == 2)
                    {
                        loc = CommonService.ConvertLatLonToDbGeography(latlng[1], latlng[0]); // lat _ lng
                    }
                }
                var dbObj = new ListItem
                {
                    Location = loc,
                    Name = source.Name,
                    Description = source.Description,
                    Phone = source.Phone,
                    LogoImage = source.LogoImage,
                    LastEdit = DateTime.Now,
                    BgImage = source.BgImage,
                    Address = source.Address,
                    Rating = source.Rating,
                    Type = source.Type,
                    Id = source.Id,
                    Status = source.Status,
                    Cords = loc,
                    CreationDate = DateTime.Now
                };
                dbContext.ListItems.Add(dbObj);
                dbContext.SaveChanges();
            }
        }

        public static string AddRemove_Favourite_Restaurent(AddFavtItemRequestModel source)
        {
            using (var dbContext = new DeliversEntities())
            {
                var rest = dbContext.ListItems.FirstOrDefault(i => i.Id.ToString() == source.ItemId);
                if (rest != null)
                {
                    var isfavtExist = dbContext.ListItems_Favt.FirstOrDefault(i => i.ItemId.ToString() == source.ItemId
                                      && i.UserId == source.UserId);

                    if (source.IsFavourite)
                    {
                        if (isfavtExist == null)
                        {
                            var obj = new ListItems_Favt
                            {
                                ItemId = Convert.ToInt64(source.ItemId),
                                DateTime = DateTime.Now,
                                UserId = source.UserId
                            };
                            dbContext.ListItems_Favt.Add(obj);
                            dbContext.SaveChanges();
                            return "Item added as favourite";
                        }
                        else
                        {
                            return "Item already is in favourite";
                        }
                    }
                    else
                    {
                        if (isfavtExist != null)
                        {
                            dbContext.ListItems_Favt.Remove(isfavtExist);
                            dbContext.SaveChanges();
                            return "Item removed from favourite";
                        }
                        return "Item is not in favourite";
                    }
                }
                return "Invalid item";
            }
        }

        public static GetListResponseModel GetI_Favourite_temsForList(GetFavouriteListRequestModel requestModel)
        {
            DbGeography userLoc = null;
            List<string> latlng = new List<string>();
            if (!string.IsNullOrEmpty(requestModel.Cords) && requestModel.Cords != "")
            {
                latlng = requestModel.Cords.Split('_').ToList();
                if (latlng.Count == 2)
                {
                    userLoc = CommonService.ConvertLatLonToDbGeography(latlng[1], latlng[0]); // lat _ lng
                }
            }
            using (var dbContext = new DeliversEntities())
            {
                requestModel.CurrentPage--;
                var response = new GetListResponseModel();
                var newList = new List<ListItemLocal>();

                string searchText = null;
                if (!string.IsNullOrEmpty(requestModel.SearchTerm))
                {
                    searchText = requestModel.SearchTerm.ToLower();
                }

                var list = dbContext.ListItems_Favt.Where(item =>
                 (string.IsNullOrEmpty(searchText) ||
                 item.ListItem.Name.ToLower().Contains(searchText) ||
                 item.ListItem.Description.ToLower().Contains(searchText) ||
                 (item.ListItem.ItemDetails.Any(det => det.Name.ToLower().Contains(searchText)))
                 )).ToList();
                if (list.Any())
                {
                    var take = list.Skip(requestModel.CurrentPage * requestModel.ItemsPerPage).
                        Take(requestModel.ItemsPerPage).ToList();
                    if (take.Any())
                    {
                        var finals = take.Select(obj => obj.ListItem.MapListItem()).ToList();
                        ///
                        foreach (var rest in finals)
                        {
                            var dist = CommonService.GetDistance((double)userLoc.Latitude, (double)userLoc.Longitude, Convert.ToDouble(rest.LocationObj.Latitude), Convert.ToDouble(rest.LocationObj.Longitude));
                            //  if ((int)dist < Convert.ToInt16(20))
                            {
                                var disst = Math.Round((double)dist, 2);
                                rest.LocationObj = null;
                                rest.Distance = disst;
                                rest.Name = rest.Name;
                                newList.Add(rest);
                            }
                        }
                        response.Items = newList.OrderBy(obj => obj.Distance).ToList();
                    }
                }
                response.ItemsPerPage = requestModel.ItemsPerPage;
                response.CurrentPage++;
                response.TotalItems = list.Count;
                return response;
            }
        }
    }
}
