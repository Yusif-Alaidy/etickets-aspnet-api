using etickets_aspnet_api.Utility.DBInitializer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace etickets_aspnet_api.Utility.DBInitializer
{
    public class DBInitializer:IDBInitializer
    {
        #region Fields
        public CineBookContext Context           { get; }
        public UserManager<ApplicationUser> User { get; }
        public RoleManager<IdentityRole> Role    { get; }
        #endregion

        #region Constructore
        public DBInitializer(CineBookContext context, UserManager<ApplicationUser> user, RoleManager<IdentityRole> role) 
        {
            this.Context = context;
            this.User    = user;
            this.Role    = role;
        }
        #endregion

        public void Initialize()
        {
            // Create Update-database --------------------------------
            if (Context.Database.GetPendingMigrations().Any())
            {
                Context.Database.Migrate();
            }
            // -------------------------------------------------------
            
            // Seed data for role --------------------------------------------------------
            if (Context.Roles is null)
            {
                Role.CreateAsync(new(SD.SuperAdminRole)).GetAwaiter().GetResult();
                Role.CreateAsync(new(SD.AdminRole)).GetAwaiter().GetResult();
                Role.CreateAsync(new(SD.CustomerRole)).GetAwaiter().GetResult();
                Role.CreateAsync(new(SD.CompanyRole)).GetAwaiter().GetResult();

                User.CreateAsync(new()
                {
                    Name            = "Admin",
                    UserName        = "Admin",
                    Email           = "Admin@gmail.com",
                    EmailConfirmed  = true,
                    
                }, "Admin@123").GetAwaiter().GetResult();
                var user = User.FindByNameAsync("Admin").GetAwaiter().GetResult();
                user.EmailConfirmed = true;
                if (user is not null)
                    User.AddToRoleAsync(user, SD.SuperAdminRole).GetAwaiter().GetResult();  
            }
            // ---------------------------------------------------------------------------
        }

    }
}
