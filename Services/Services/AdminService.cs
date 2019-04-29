using Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Services
{
    public class MobileContentsClass
    {
        public string Text { get; set; }
        public string Phone { get; set; }
    }


    public static class AdminService
    {
        public static bool AddUserIssue(UserIssueRequestModel model)
        {
            return true;
        }

        public static MobileContentsClass GetMobileContents()
        {
            return new MobileContentsClass
            {
                Text= "Thank you for choosing Delivers.pk. We will be launching our food delivery services before Rumzan in Gujranwala city.\n Want to be our partner?",
                Phone="+92553828677"
            };
        }

    }
}
