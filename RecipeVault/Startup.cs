using CloudinaryDotNet;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RecipeVault.Data;
using RecipeVault.Services;
using EasyData.Services;
using Microsoft.AspNetCore.Authentication;


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
            
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly",
                    policy => policy.RequireRole("Admin"));
            });

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
            app.UseAuthentication();
            app.UseAuthorization();
            
            
            app.UseWhen(ctx => ctx.Request.Path.StartsWithSegments("/easydata"),
                adminApp =>
                {
                    // runs *only* for URLs that begin with /easyadmin…

                    adminApp.Use(async (ctx, next) =>
                    {
                        // 1) not signed in  → kick off the normal Identity login flow
                        if (!ctx.User.Identity?.IsAuthenticated ?? true)
                        {
                            await ctx.ChallengeAsync();          // 302 → /Identity/Account/Login
                            return;
                        }

                        // 2) signed in but *not* an Admin  → 403 Forbidden
                        if (!ctx.User.IsInRole("Admin"))
                        {
                            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                            await ctx.Response.CompleteAsync();
                            return;
                        }

                        // 3) good to go
                        await next();
                    });
                });

            // ✅ EasyData + Razor Pages
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapEasyData(options =>
                {
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
