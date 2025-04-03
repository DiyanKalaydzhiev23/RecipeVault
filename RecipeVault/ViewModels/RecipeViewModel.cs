using RecipeVault.Enums;
using System.ComponentModel.DataAnnotations;

namespace RecipeVault.ViewModels
{
    public class RecipeViewModel
    {
        public int Id { get; set; } // Needed for Edit

        [Required]
        public string Name { get; set; }

        [Required]
        public RecipeDifficultyLevel Difficulty { get; set; }

        [Required]
        public int PreparationTime { get; set; }

        public IFormFile? ImageFile { get; set; } // Optional for edit

        [Required]
        public string Instructions { get; set; }

        public List<string> IngredientNames { get; set; } = new();
    }
}