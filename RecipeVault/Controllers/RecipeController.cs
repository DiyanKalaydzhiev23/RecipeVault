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
        
        // Renders the Create view
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST /Recipe/Create
        [HttpPost]
        public async Task<IActionResult> Create(RecipeViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model); // Assumes form posts from a Razor view

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

            foreach (var ingredientName in model.IngredientNames
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var existing = await _context.Ingredients
                    .FirstOrDefaultAsync(i => i.Name.ToLower() == ingredientName.ToLower());

                var ingredient = existing ?? new Ingredient { Name = ingredientName };

                if (existing == null)
                {
                    _context.Ingredients.Add(ingredient);
                    await _context.SaveChangesAsync();
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

        [HttpGet("Recipe/api/{name}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRecipeJson(string name)
        {
            var recipes = await _context.Recipes
                .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient)
                .Where(r => r.Name.ToLower() == name.ToLower())
                .ToListAsync();

            if (recipes == null || recipes.Count == 0)
                return NotFound(new { meals = (object)null });

            var meals = new List<Dictionary<string, object>>();

            foreach (var recipe in recipes)
            {
                var meal = new Dictionary<string, object>
                {
                    ["id"] = recipe.Id,  // ✅ Add this line
                    ["name"] = recipe.Name,
                    ["image"] = recipe.ImageUrl,
                    ["instructions"] = recipe.Instructions,
                    ["userId"] = recipe.UserId,
                };

                var ingredients = recipe.RecipeIngredients
                    .Select(ri => ri.Ingredient.Name)
                    .ToList();

                meal["ingredients"] = ingredients;

                meals.Add(meal);
            }

            return Ok(new { meals });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var recipe = await _context.Recipes
                .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null) return NotFound();

            var model = new RecipeViewModel
            {
                Id = recipe.Id,
                Name = recipe.Name,
                Difficulty = recipe.Difficulty,
                PreparationTime = recipe.PreparationTime,
                IngredientNames = recipe.RecipeIngredients.Select(ri => ri.Ingredient.Name).ToList(),
                Instructions = recipe.Instructions
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(RecipeViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var recipe = await _context.Recipes
                .Include(r => r.RecipeIngredients)
                .FirstOrDefaultAsync(r => r.Id == model.Id);

            if (recipe == null) return NotFound();

            recipe.Name = model.Name;
            recipe.Difficulty = model.Difficulty;
            recipe.PreparationTime = model.PreparationTime;
            recipe.Instructions = model.Instructions;

            // Update ingredients
            recipe.RecipeIngredients.Clear();

            foreach (var ingredientName in model.IngredientNames.Where(n => !string.IsNullOrWhiteSpace(n)).Distinct())
            {
                var existing = await _context.Ingredients.FirstOrDefaultAsync(i => i.Name.ToLower() == ingredientName.ToLower());
                var ingredient = existing ?? new Ingredient { Name = ingredientName };

                if (existing == null) _context.Ingredients.Add(ingredient);

                recipe.RecipeIngredients.Add(new RecipeIngredient
                {
                    Recipe = recipe,
                    Ingredient = ingredient
                });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }
        
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var recipe = await _context.Recipes.FindAsync(id);

            if (recipe == null) return NotFound();

            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}
