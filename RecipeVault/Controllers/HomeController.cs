using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RecipeVault.Models;

namespace RecipeVault.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<IdentityUser> _userManager;

    public HomeController(
        ILogger<HomeController> logger,
        UserManager<IdentityUser> userManager
    )
    {
        _logger = logger;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        if (User.Identity.IsAuthenticated)
        {
            ViewData["CurrentUserId"] = _userManager.GetUserId(User);
        }
        else
        {
            ViewData["CurrentUserId"] = string.Empty;
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}