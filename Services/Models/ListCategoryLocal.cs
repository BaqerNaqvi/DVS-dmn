using Services.DbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models
{
   public class ListCategoryLocal
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Status { get; set; }
        public System.DateTime CreationTime { get; set; }
        public Nullable<int> CatId { get; set; }

        public int ItemCount { get
                ;set;
        }


    }

    public static class CatMapper {

        public static ListCategoryLocal Mapper(this ListCategory source)
        {
            return new ListCategoryLocal
            {
                Id = source.Id,
                CatId= source.CatId,
                CreationTime = source.CreationTime,
                Description= source.Description,
                ItemCount=0,
                Name= source.Name,
                Status= source.Status
            };
        }
    }

}
