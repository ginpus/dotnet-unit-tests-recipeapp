using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Extensions;
using Domain.Models;
using Persistence.Filters;
using Persistence.Models.WriteModels;
using Persistence.Repositories;

namespace Domain.Services
{
    public class RecipeService : IRecipeService
    {
        private readonly IRecipesRepository _recipesRepository;
        private readonly IDescriptionsRepository _descriptionsRepository;

        public RecipeService(IRecipesRepository recipesRepository, IDescriptionsRepository descriptionsRepository)
        {
            _recipesRepository = recipesRepository;
            _descriptionsRepository = descriptionsRepository;
        }

        public async Task<IEnumerable<Recipe>> GetAllAsync(RecipesFilter recipesFilter)
        {
            var recipes = await _recipesRepository.GetAll(recipesFilter);

            return recipes.Select(recipe => recipe.MapToRecipe());
        }

        public async Task<int> CreateAsync(Recipe model)
        {
            var insertRecipeTask = _recipesRepository.SaveAsync(new RecipeWriteModel
            {
                Id = model.Id,
                Name = model.Name,
                Difficulty = model.Difficulty,
                TimeToComplete = model.TimeToComplete,
                DateCreated = model.DateCreated
            });

            var insertDescriptionTask = _descriptionsRepository.SaveAsync(new DescriptionWriteModel
            {
                RecipeId = model.Id,
                Description = model.Description
            });

            await Task.WhenAll(insertRecipeTask, insertDescriptionTask);

            return await insertRecipeTask;
        }

        public async Task<int> EditAsync(int id, string name, string description)
        {
            var editNameTask = _recipesRepository.EditNameAsync(id, name);
            var editDescriptionTask = _descriptionsRepository.EditDescriptionAsync(id, description);

            await Task.WhenAll(editNameTask, editDescriptionTask);

            return await editNameTask;
        }

        public async Task<int> DeleteByIdAsync(int id)
        {
            var deleteRecipeTask = _recipesRepository.DeleteByIdAsync(id);
            var deleteDescriptionTask = _descriptionsRepository.DeleteByIdAsync(id);

            await Task.WhenAll(deleteRecipeTask, deleteDescriptionTask);

            return await deleteDescriptionTask;
        }

        public async Task<int> DeleteAllAsync()
        {
            var deleteAllRecipesTask = _recipesRepository.DeleteAllAsync();
            var deleteAllDescriptionsTask = _descriptionsRepository.DeleteAllAsync();

            await Task.WhenAll(deleteAllRecipesTask, deleteAllDescriptionsTask);

            return await deleteAllRecipesTask;
        }
    }
}