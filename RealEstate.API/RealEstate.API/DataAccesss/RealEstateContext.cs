using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using RealEstate.API.Models;
namespace RealEstate.API.DataAccesss
{
    public class MyDbContextInitializer : DropCreateDatabaseIfModelChanges<RealEstateContext>
    {
        protected override void Seed(RealEstateContext dbContext)
        {
            // seed data
            User user = new User();
            user.FirstName = "Vishal";
            user.LastName = "Khatal";

            dbContext.User.Add(user);
            dbContext.SaveChanges();
            base.Seed(dbContext);
        }
    }

    public class RealEstateContext : Microsoft.AspNet.Identity.EntityFramework.IdentityDbContext<ApplicationUser>
    {
        public RealEstateContext()
            : base("RealEstateContext", throwIfV1Schema: false)
        {
            this.Configuration.LazyLoadingEnabled = false;
            this.Configuration.ProxyCreationEnabled = false;
        }

        #region Application Table
        public DbSet<User> User { get; set; }
        public DbSet<Property> Property { get; set; }
        public DbSet<PropertyPhoto> PropertyPhotos { get; set; }
        public DbSet<PropertyFeedBack> PropertyFeedBack { get; set; }
        public DbSet<Message> Message { get; set; }
        public DbSet<NewsLetter> NewsLetter { get; set; }
        public DbSet<NewsLetterSubscriber> NewsLetterSubscriber { get; set; }
             #endregion
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
           // Database.SetInitializer(new MigrateDatabaseToLatestVersion<RealEstateContext, RealEstate.API.Migrations.Configuration>());
            modelBuilder.Properties<DateTime>().Configure(c => c.HasColumnType("datetime2"));

            //Database.SetInitializer(new MyDbContextInitializer());
            base.OnModelCreating(modelBuilder);
        }
        public static RealEstateContext Create()
        {
            return new RealEstateContext();
        }
    }
}