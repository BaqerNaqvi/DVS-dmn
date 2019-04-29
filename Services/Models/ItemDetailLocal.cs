using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.DbContext;
using Services.Services;

namespace Services.Models
{
    public class ItemDetailLocal
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public long ListItemId { get; set; }
        public string CreationDate { get; set; }
        public System.DateTime EditDate { get; set; }
        public string Image { get; set; }
        public bool Status { get; set; }

        public string Description { get; set; }

        public virtual ListItemLocal_Short ListItem { get; set; }

        public virtual List<RatingLocal> Reviewes { get; set; }
        public string Type { get; set; }


    }


    public class ItemDetailLocal_Short
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
    
        public string Image { get; set; }


    }

    public static class ItemDetailMap
    {
        public static ItemDetailLocal ItemDetailMapper(this ItemDetail source)
        {
            var rr = source.ListItem;
            return new ItemDetailLocal
            {
                Id = source.Id,
                Status = source.Status,
                CreationDate = source.CreationDate.ToShortDateString() + " " + source.CreationDate.ToShortTimeString(),
                Name = source.Name,
                EditDate = source.EditDate,
                Image = source.Image,
               ListItem = source.ListItem.MapListItem_ShortM(),
                ListItemId = source.ListItemId,
                Price = source.Price,
                Description = source.Description,
                Type= source.Type
              //  Reviewes = ReviewService.GetReviewsByItemId(source.Id, 2) // 1 for lists items , 2 for detail items 
                
            };
        }

        public static ItemDetailLocal_Short ItemDetailShortMapper(this ItemDetail source)
        {
            return new ItemDetailLocal_Short
            {
                Id = source.Id,
                Name = source.Name,
                Image = source.Image,
                Price = source.Price,
            };
        }
    }
}
