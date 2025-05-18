// Program.cs  ────────────────────────────────────────────────────────────
using CloudinaryDotNet;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RecipeVault.Data;
using RecipeVault.Services;
using EasyData.Services;                    
using Microsoft.AspNetCore.Authentication;  
using Microsoft.AspNetCore.Authorization;   
using Microsoft.AspNetCore.Http;            

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// ─── Identity ───────────────────────────────────────────────────────────
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// ───  “AdminOnly” authorization policy  ────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly",
        policy => policy.RequireRole("Admin"));
});

// ─── Cloudinary ─────────────────────────────────────────────────────────
var cloudinary = new Cloudinary(new Account(
    Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME"),
    Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY"),
    Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET")
));
builder.Services.AddSingleton(cloudinary);  // NEW (register the instance)
builder.Services.AddSingleton<CloudinaryService>();

// ─── Razor / MVC ────────────────────────────────────────────────────────
builder.Services.AddRazorPages(options =>                           
{
    options.Conventions.AuthorizeFolder("/easyadmin", "AdminOnly"); 
});

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

var app = builder.Build();

// ─── Middleware pipeline ────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ─── Protect *everything* that starts with /easyadmin/  (API, assets…) ──
app.UseWhen(ctx => ctx.Request.Path.StartsWithSegments("/easyadmin"), // NEW
    branch =>
    {
        branch.Use(async (ctx, next) =>
        {
            if (!ctx.User.Identity?.IsAuthenticated ?? true)
            {
                await ctx.ChallengeAsync();   // 302 → login
                return;
            }

            if (!ctx.User.IsInRole("Admin"))
            {
                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                await ctx.Response.CompleteAsync();
                return;
            }

            await next();
        });
    });
// ────────────────────────────────────────────────────────────────────────

// EasyData REST API (add the policy so its calls need Admins too)
app.MapEasyData(options =>
{
    options.UseDbContext<ApplicationDbContext>();
}).RequireAuthorization("AdminOnly");                       

app.MapRazorPages();

// Normal MVC controllers
app.MapStaticAssets();
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
