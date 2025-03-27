using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using RecipeVault.Enums;

namespace RecipeVault.Models
{
    public class Recipe
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public RecipeDifficultyLevel Difficulty { get; set; }

        [Required]
        [Display(Name = "Preparation Time (minutes)")]
        public int PreparationTime { get; set; }

        public string ImageUrl { get; set; }

        // Relation to IdentityUser
        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public IdentityUser User { get; set; }
        
        public ICollection<RecipeIngredient> RecipeIngredients { get; set; }
    }
}