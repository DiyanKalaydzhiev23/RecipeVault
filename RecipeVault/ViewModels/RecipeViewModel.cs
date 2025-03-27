using System.ComponentModel.DataAnnotations;
using RecipeVault.Enums;

namespace RecipeVault.ViewModels
{
    public class RecipeViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public RecipeDifficultyLevel Difficulty { get; set; }

        [Required]
        [Display(Name = "Preparation Time (minutes)")]
        public int PreparationTime { get; set; }

        [Required]
        public IFormFile ImageFile { get; set; }
    }
}