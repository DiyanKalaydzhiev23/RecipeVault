using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RecipeVault.Models;
using RecipeVault.Services;
using RecipeVault.ViewModels;
using System.Threading.Tasks;

namespace RecipeVault.Controllers
{
    [Authorize]
    public class RecipeController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly CloudinaryService _cloudinary;

        public RecipeController(UserManager<IdentityUser> userManager, CloudinaryService cloudinary)
        {
            _userManager = userManager;
            _cloudinary = cloudinary;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(RecipeViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var imageUrl = await _cloudinary.UploadImageAsync(model.ImageFile);
            var userId = _userManager.GetUserId(User);

            var recipe = new Recipe
            {
                Name = model.Name,
                Difficulty = model.Difficulty,
                PreparationTime = model.PreparationTime,
                ImageUrl = imageUrl,
                UserId = userId
            };

            // Save to DB (add via DbContext)
            return RedirectToAction("Index", "Home");
        }
    }
}