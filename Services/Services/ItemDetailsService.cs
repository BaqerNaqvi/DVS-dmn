using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.DbContext;
using Services.Models;

namespace Services.Services
{
    public static class ItemDetailsService
    {
        public static ItemDetailLocal GetItemDetailLocalById(long itemId)
        {
            using (var dbcontext = new DeliversEntities())
            {
                var items = dbcontext.ItemDetails.FirstOrDefault(det => det.Id == itemId);
                return items?.ItemDetailMapper();
            }
        }
    }
}
