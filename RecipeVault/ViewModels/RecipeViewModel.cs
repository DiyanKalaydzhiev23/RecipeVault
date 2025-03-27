using RecipeVault.Enums;
using System.ComponentModel.DataAnnotations;

namespace RecipeVault.ViewModels
{
    public class RecipeViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public RecipeDifficultyLevel Difficulty { get; set; }

        [Required]
        public int PreparationTime { get; set; }

        [Required]
        public IFormFile ImageFile { get; set; }

        public List<string> IngredientNames { get; set; } = new();
    }
}