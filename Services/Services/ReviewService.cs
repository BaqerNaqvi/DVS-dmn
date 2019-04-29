using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.DbContext;
using Services.Models;

namespace Services.Services
{
    public static class ReviewService
    {

        public static bool AddReview(RatingLocal source)
        {
            using (var dbContext = new DeliversEntities())
            {
                try
                {
                    var dbObj = new Rating
                    {
                        Id = 0,
                        DateTime = DateTime.Now,
                        RatedToItem = source.RatedToItem,
                        IsApproved = true,
                        Comments = source.Comments,
                        RatingStar = source.RatingStar,
                        RatedByUserId = source.RatedByUserId
                    };
                    dbContext.Ratings.Add(dbObj);
                    dbContext.SaveChanges();
                }
                catch (Exception df)
                {
                    return false;
                }
            }
            return true;
        }

    }
}
