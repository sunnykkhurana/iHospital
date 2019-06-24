using System;
using System.Linq;
using iHospital.Database;
using iHospital.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(iHospital.Startup))]
namespace iHospital
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            CreateRolesandUsers();
        }

        //Create default User roles and Admin user for login   
        private void CreateRolesandUsers()
        {
            ApplicationDbContext context = new ApplicationDbContext();

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            //Creat Admin role and 1st Admin user
            if (!roleManager.RoleExists("Admin"))
            {
                //Create Admin role
                var roleAdmin = new IdentityRole
                {
                    Id = "0ded18bc-cead-489a-b107-0fadb1cdf962",
                    Name = "Admin"
                };
                roleManager.Create(roleAdmin);

                //Define 1st Admin super user who will maintain the website 
                var user = new ApplicationUser
                {
                    UserName = "andreispham86@gmail.com",
                    Email = "andreispham86@gmail.com"
                };
                var userPWD = "Pwd!23";

                //Create 1st Admin user
                var chkUser = userManager.Create(user, userPWD);

                //Add user to role Admin
                if (chkUser.Succeeded)
                {
                    var result = userManager.AddToRole(user.Id, "Admin");
                }
            }

            //Create AdministrativeStaff role
            if (!roleManager.RoleExists("AdministrativeStaff"))
            {
                var role = new IdentityRole
                {
                    Id = "dd9bd5de-674d-4f55-b526-d945d9456da7",
                    Name = "AdministrativeStaff"
                };
                roleManager.Create(role);
            }

            //Create MedicalStaff role
            if (!roleManager.RoleExists("MedicalStaff"))
            {
                var role = new IdentityRole
                {
                    Id = "90f71088-0b87-43d1-8b69-d3fce07cac72",
                    Name = "MedicalStaff"
                };
                roleManager.Create(role);
            }

            //Create Patient role    
            if (!roleManager.RoleExists("Patient"))
            {
                var role = new IdentityRole
                {
                    Id = "3929b37d-88ba-45db-b727-cba789fd2833",
                    Name = "Patient"
                };
                roleManager.Create(role);
            }

            var dbcontext = new iHospitalDataContext();
            if (dbcontext.Departments.FirstOrDefault(d => d.Id == "test") == null)
            {
                var d = new Department()
                {
                    Id = "test",
                    DateModified = DateTime.Now,
                    DateCreated = DateTime.Now,
                    Name = "test"
                };
                dbcontext.Departments.InsertOnSubmit(d);
                dbcontext.SubmitChanges();
            }
        }
    }
}