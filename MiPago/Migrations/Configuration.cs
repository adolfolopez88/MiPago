namespace MiPago.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<MiPago.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(MiPago.Models.ApplicationDbContext context)
        {
            var role_manager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var user_manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            var user = new ApplicationUser { UserName = "desarrollo@consultis.cl", Email= "desarrollo@consultis.cl" };

            user_manager.Create(user, "en66wRpZtmbF_Y");
            role_manager.Create(new IdentityRole { Name = "Administrador" });
            role_manager.Create(new IdentityRole { Name = "Usuario" });
            user_manager.AddToRole(user.Id, "Administrador");
        }
    }
}
