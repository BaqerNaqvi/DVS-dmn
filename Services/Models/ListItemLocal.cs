using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.DbContext;
using Services.Services;
using System.Data.Entity.Spatial;
using System.ComponentModel.DataAnnotations;

namespace Services.Models
{
    public class ListItemLocal
    {
        public long Id { get; set; }
        public int Type { get; set; }

        public string TypeName { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }

        public DbGeography LocationObj { get; set; }

        public string Phone { get; set; }
        public string Rating { get; set; }
        public string Address { get; set; }
        public string LogoImage { get; set; }

        public string Cords { get; set; } // lat_long

        public string BgImage { get; set; }
        public bool Status { get; set; }
        public string CreationDate { get; set; }
        public System.DateTime LastEdit { get; set; }

        public virtual List<RatingLocal> Reviewes { get; set; }

        //
        public double Distance { get; set; }

        public string Opens { get; set; }
        public string Closes { get; set; }
        public Nullable<int> MinOrder { get; set; }

        [Display(Name = "Featured")]
        public bool IsFeatured { get; set; }

        // FOR CREATING RESTARANT IN ADMIN
        public List<ListCategoryLocal> Categoreis { get; set; }

    }

    public class ListItemLocal_Short
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }

        public string LogoImage { get; set; }  
    }


    public static class ListItemMapper
    {
        public static ListItemLocal MapListItem(this ListItem source)
        {
            var loc = "";
            if (source.Cords != null)
            {
                loc = source.Cords.Latitude + "_" + source.Cords.Longitude;
            }

            var rating = new List<RatingLocal>();
            var starts = 0.0;         
            if (source.Rating != null && source.Rating.Any())
            {
                rating = source.Ratings.Select(r => r.MappReviewe()).ToList();
                if (rating.Any())
                {
                    starts = rating.Sum(y => y.RatingStar) / rating.Count;
                }
            }
            Random rnd = new Random();
            int img = rnd.Next(1, 5);

            return new ListItemLocal
            {
                Address = source.Address,
                BgImage = "http://www.delivers.pk/Images/Rest/Large/large_01.jpg", //source.BgImage,
                CreationDate = source.CreationDate.ToShortDateString() +" "+source.CreationDate.ToShortTimeString(),
                LastEdit = source.LastEdit,
                Description = source.Description,
                Id = source.Id,
                LocationObj = source.Location,
                LogoImage = "http://www.delivers.pk/Images/Rest/Small/small_0"+img+".jpg", //source.LogoImage,
                Name = source.Name,
                Phone = source.Phone,
                Rating = starts.ToString(),                   //source.Rating,
                Status = source.Status,
                Type = source.Type,
                Cords = loc,
                Reviewes = rating,
                Opens= source.Opens,
                Closes= source.Closes,
                MinOrder= source.MinOrder,
                IsFeatured= false
            };
        }

        public static ListItemLocal_Short MapListItem_ShortM(this ListItem source)
        {
            return new ListItemLocal_Short
            {               
                Id = source.Id,
                Address = source.Address,
                LogoImage = source.LogoImage,
                Name = source.Name,                
            };
        }
    }
}
