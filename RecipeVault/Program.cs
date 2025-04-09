using CloudinaryDotNet;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RecipeVault.Data;
using RecipeVault.Services;
using EasyData.Services; // ✅ Needed for MapEasyData

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Cloudinary
var cloudinary = new Cloudinary(new Account(
    Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME"),
    Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY"),
    Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET")
));

builder.Services.AddSingleton<CloudinaryService>();

// Razor + Controllers
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapEasyData(options => {
    options.UseDbContext<ApplicationDbContext>();
});

app.MapRazorPages();

// Normal controller routes
app.MapStaticAssets();
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();