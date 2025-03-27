using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RecipeVault.Models
{
    public class Ingredient
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public ICollection<RecipeIngredient> RecipeIngredients { get; set; }
    }
}