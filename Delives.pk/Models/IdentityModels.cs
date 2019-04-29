using System;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Services.Models;
using Services.Services;
using System.Data.Entity.Spatial;

namespace Delives.pk.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime EditTime { get; set; }
        public int Type { get; set; }
        public bool IsApproved { get; set; }

        public string CNIC { get; set; }

        public bool Status { get; set; }

        public string Address { get; set; }

        public DbGeography Location { get; set; }

    }


    public static class UserMapper
    {
        public static UserLocal MappUser(this ApplicationUser source)
        {
            var cords = "";
            if (source.Location != null)
            {
                cords = source.Location.Latitude + "_" + source.Location.Longitude;
            }
            return new UserLocal
            {
                Id = source.Id,
                FirstName = source.FirstName,
                LastName = source.LastName,

                Email = source.Email,
                EmailConfirmed = source.EmailConfirmed,

                PhoneNumber = source.PhoneNumber,
                PhoneNumberConfirmed = source.PhoneNumberConfirmed,

                UserName = source.UserName,
                CreationTime = source.CreationTime,
                Type = source.Type,
                IsApproved = source.IsApproved,
                CNIC = source.CNIC,
                Status = source.Status,
                TypeString = CommonService.GetUserTypeString(source.Type),
                StatusString = source.Status == true ? "Active" : "InActive",
                Address= source.Address,
                Cords= cords
            };

        }

        public static UserLocal_ShortVersion MappUser_Short(this ApplicationUser source)
        {
            var cords = "";
            if (source.Location != null)
            {
                cords = source.Location.Latitude + "_" + source.Location.Longitude;
            }
            return new UserLocal_ShortVersion
            {
                Id = source.Id,
                FirstName = source.FirstName,
                LastName = source.LastName,

                Email = source.Email,
                EmailConfirmed = source.EmailConfirmed,

                PhoneNumber = source.PhoneNumber,
                PhoneNumberConfirmed = source.PhoneNumberConfirmed,

                UserName = source.UserName,
                CreationTime = source.CreationTime,
                Type = source.Type,
                IsApproved = source.IsApproved,
                CNIC = source.CNIC,
                Status = source.Status,
                TypeString = CommonService.GetUserTypeString(source.Type),
                StatusString = source.Status == true ? "Active" : "InActive",
                Address= source.Address,
                Cords= cords
            };

        }

        public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
        {
            public ApplicationDbContext()
                : base("DefaultConnection", throwIfV1Schema: false)
            {
            }

            public static ApplicationDbContext Create()
            {
                return new ApplicationDbContext();
            }
        }
    }
}