using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeVault.Models;
using RecipeVault.Services;
using RecipeVault.ViewModels;
using RecipeVault.Data;

namespace RecipeVault.Controllers
{
    [Authorize]
    public class RecipeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly CloudinaryService _cloudinary;

        public RecipeController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            CloudinaryService cloudinary)
        {
            _context = context;
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
                UserId = userId,
                RecipeIngredients = new List<RecipeIngredient>()
            };

            foreach (var ingredientName in model.IngredientNames.Where(name => !string.IsNullOrWhiteSpace(name)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var existing = await _context.Ingredients
                    .FirstOrDefaultAsync(i => i.Name.ToLower() == ingredientName.ToLower());

                var ingredient = existing ?? new Ingredient { Name = ingredientName };

                if (existing == null)
                {
                    _context.Ingredients.Add(ingredient);
                    await _context.SaveChangesAsync(); // to generate Id
                }

                recipe.RecipeIngredients.Add(new RecipeIngredient
                {
                    IngredientId = ingredient.Id,
                    Recipe = recipe
                });
            }

            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}
