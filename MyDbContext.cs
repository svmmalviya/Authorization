using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Authorization
{
    public class MyDbContext : IdentityDbContext<AppUser,AppRole,int>  // Specify User and Role
    {
        public MyDbContext(DbContextOptions<MyDbContext> options):base(options)
        {
                
        }

        //public DbSet<User> Users { get; set; }
        ////public DbSet<IdentityUser> AppUsers { get; set; }
        ////public DbSet<IdentityRole> AppRoles { get; set; }
        //public DbSet<Role> Roles { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //     modelBuilder.Entity<AppRole>().HasData(
            //    new Role { Id = 1, Name = "Admin", Description = "Admin Role" },
            //    new Role { Id = 2, Name = "User", Description = "User Role" }
            //);

            modelBuilder.Entity<AppRole>().HasData(
                new Role { Id=-1,  Name = "Admin", Description = "Admin Role" },
                new Role { Id=-2, Name = "User", Description = "User Role" }
            );


            //modelBuilder.Entity<AppUser>().HasData(
            //    new User {  Name = "Shiv", Password = "123", RoleId = 1 },
            //    new User {  Name = "Sohan", Password = "123", RoleId = 2 }
            //);



        }

    }

    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<User> users { get; set; }
    }

    public class AppUser : IdentityUser<int> {

        public AppUser():base()
        {
                
        }
    }
    public class AppRole: IdentityRole<int>
    {
        public AppRole():base()
        {
                
        }
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public int RoleId { get; set; }
        [ForeignKey("RoleId")]
        public Role Roles { get; set; }
    }

    
}
