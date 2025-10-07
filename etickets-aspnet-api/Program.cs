using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Stripe;
using System.Text;
namespace etickets_aspnet_api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            #region Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddDbContext<CineBookContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("UserDatabase")));

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(option =>
            {
                option.Password.RequiredLength = 8;      // Minimum password length
                option.User.RequireUniqueEmail = true;   // Require unique email per user
            }).AddEntityFrameworkStores<CineBookContext>()
            .AddDefaultTokenProviders();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(config => {
                config.RequireHttpsMetadata = false;
                config.SaveToken = true;
                config.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = "https://localhost:7177",
                    ValidAudience = "https://localhost:5000,https://localhost:5500,https://localhost:4200",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("EraaSoft515##EraaSoft515##EraaSoft515##EraaSoft515##")),
                    ValidateLifetime = true
                };
            });

            builder.Services.AddScoped<IDBInitializer, DBInitializer>();

            // Register custom services
            builder.Services.AddTransient<IEmailSender, EmailSender>(); // Email sending service
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>)); // Generic repository pattern

            builder.Services.ConfigureApplicationCookie(option =>
            {
                option.LoginPath = "/Identity/Account/Login";
                option.AccessDeniedPath = "/";
            });
            //builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
            StripeConfiguration.ApiKey = builder.Configuration["StripeKey:Key"];


            builder.Services.AddOpenApi();
            #endregion

            var app = builder.Build();

            #region Configure middleware pipeline

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }


            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            using (var scope = app.Services.CreateScope())
            {
                var dbInitializer = scope.ServiceProvider.GetRequiredService<IDBInitializer>();
                // use dbInitializer
                dbInitializer.Initialize();
            }

            app.MapControllers();
            #endregion

            app.Run();
        }
    }
}
