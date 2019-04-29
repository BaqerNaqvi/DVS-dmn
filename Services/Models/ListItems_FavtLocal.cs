using Services.DbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models
{
    public class ListItems_FavtLocal
    {
        public long Id { get; set; }
        public long ItemId { get; set; }
        public string UserId { get; set; }
        public DateTime DateTime { get; set; }

        public virtual ListItemLocal ListItem { get; set; }
    }

    public static class ItemMapper
    {
        public static ListItems_FavtLocal Mapper_ListItems_FavtLocal(this ListItems_Favt source)
        {
            return new ListItems_FavtLocal
            {
                Id = source.Id,
                ItemId = source.ItemId,
                UserId= source.UserId,
                DateTime = source.DateTime,
                ListItem = source.ListItem.MapListItem()
            };
        }
    }
}
