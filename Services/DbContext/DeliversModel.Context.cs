﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Services.DbContext
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class DeliversEntities : DbContext
    {
        public DeliversEntities()
            : base("name=DeliversEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<AspNetRole> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }
        public virtual DbSet<ItemDetail> ItemDetails { get; set; }
        public virtual DbSet<ListItem> ListItems { get; set; }
        public virtual DbSet<OrderDetail> OrderDetails { get; set; }
        public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
        public virtual DbSet<Rating> Ratings { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<ListItems_Favt> ListItems_Favt { get; set; }
        public virtual DbSet<App> Apps { get; set; }
        public virtual DbSet<Token> Tokens { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<User_Rest_Map> User_Rest_Map { get; set; }
        public virtual DbSet<Rider_Location_Map> Rider_Location_Map { get; set; }
        public virtual DbSet<OrderHistory> OrderHistories { get; set; }
        public virtual DbSet<ListCategory> ListCategories { get; set; }
        public virtual DbSet<Sm> Sms { get; set; }
    }
}
