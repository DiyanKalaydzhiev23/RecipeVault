using CloudinaryDotNet;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RecipeVault.Data;
using RecipeVault.Services;
using EasyData.Services;


namespace RecipeVault
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            DotNetEnv.Env.Load(); 
            
            var connectionString = System.Environment.GetEnvironmentVariable("DB_CONNECTION");
            
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddSingleton(new Cloudinary(new Account(
                System.Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME"),
                System.Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY"),
                System.Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET")
            )));

            services.AddSingleton<CloudinaryService>();

            services.AddRazorPages();
            services.AddControllersWithViews();
            services.AddHttpClient();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (!env.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthorization();

            // ✅ EasyData + Razor Pages
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapEasyData(options => {
                    options.UseDbContext<ApplicationDbContext>();
                });

                endpoints.MapRazorPages();
            });

            // Optionally apply migrations or seed DB
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.Migrate();
                // Seed data here if needed
            }
        }
    }
}
