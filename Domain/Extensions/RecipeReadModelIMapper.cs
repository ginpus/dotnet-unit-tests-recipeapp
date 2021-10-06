using Domain.Models;
using Persistence.Models.ReadModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Extensions
{
    public static class RecipeReadModelIMapper
    {
        public static Recipe MapToRecipe(this RecipeReadModel model)
        {
            return new Recipe
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                Difficulty = model.Difficulty,
                TimeToComplete = model.TimeToComplete,
                DateCreated = model.DateCreated
            };
        }
    }
}
