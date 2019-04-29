using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.DbContext;

namespace Services.Models
{
    public class RatingLocal
    {
        public long Id { get; set; }
        public double RatingStar { get; set; }
        public string Comments { get; set; }
        public long RatedToItem { get; set; }
        public string RatedByUserName { get; set; }
        public string CreationDate { get; set; }
        public bool IsApproved { get; set; }
        public string ReviewedOnName { get; set; }
        public string RatedByUserId { get; set; }
    }

    public static class ReviewMapper
    {
        public static RatingLocal MappReviewe(this Rating source)
        {
            return new RatingLocal
            {
                CreationDate = source.DateTime.ToShortDateString() + " " + source.DateTime.ToShortTimeString(),
                Id = source.Id,
                RatingStar = source.RatingStar,
                RatedByUserName = source.AspNetUser.FirstName + " "+ source.AspNetUser.LastName,
                RatedToItem = source.RatedToItem,
                ReviewedOnName = source.ListItem.Name,
                Comments = source.Comments,
                RatedByUserId = source.RatedByUserId
            };
        }
    }
}
