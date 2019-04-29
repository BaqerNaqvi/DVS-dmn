using Services.DbContext;
using Services.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models
{
    public class UserLocal
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTime LockoutEndDateUtc { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime EditTime { get; set; }
        public int? Type { get; set; }


        public String TypeString { get; set; }

        public bool IsApproved { get; set; }

        public string CNIC { get; set; }

        public bool? Status { get; set; }

        public string StatusString { get; set; }

        public string ProfileImageUrl { get; set; }

        public ListItemLocal Restaurant { get; set; }

        public string Address { get; set; }
        public System.Data.Entity.Spatial.DbGeography Location { get; set; }

        public string Cords { get; set; } // Lat_lng
    }

    public static class Usermapper
    {
        public static UserLocal Mapper(this AspNetUser user)
        {
            var cords = "";
            if (user.Location != null)
            {
                cords = user.Location.Latitude + "_" + user.Location.Longitude;
            }

            return new UserLocal
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                Type= user.Type,
                CNIC= user.CNIC,
                TypeString= CommonService.GetUserTypeString(user.Type),
                Status= user.Status,
                StatusString=  user.Status==true?"Active":"InActive",
                Address= user.Address,
                Cords= cords

            };
        }

        public static UserLocal_ShortVersion Mapper_Short(this AspNetUser user)
        {
            var cords = "";
            if (user.Location != null)
            {
                cords = user.Location.Latitude + "_" + user.Location.Longitude;
            }
            return new UserLocal_ShortVersion
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                Type = user.Type,
                CNIC = user.CNIC,
                TypeString = CommonService.GetUserTypeString(user.Type),
                Status = user.Status,
                StatusString = user.Status == true ? "Active" : "InActive",
                Address = user.Address,
                Cords = cords
            };
        }
    }

    public class UserLocal_ShortVersion
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public string UserName { get; set; }
        public DateTime CreationTime { get; set; }
        public int? Type { get; set; }
        public String TypeString { get; set; }

        public bool IsApproved { get; set; }

        public string CNIC { get; set; }

        public bool? Status { get; set; }

        public string StatusString { get; set; }

        public string ProfileImageUrl { get; set; }

        public string Address { get; set; }

        public string Cords { get; set; } // Lat_lng

    }



}
