using Contracts.Enums;
using Domain.Models;
using Domain.Services;
using Moq;
using Moq.Language;
using Moq.Language.Flow;
using Persistence.Filters;
using Persistence.Models.ReadModels;
using Persistence.Models.WriteModels;
using Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Domain.UnitTests.Services
{
    public class ReceipeService_Should
    {
        private readonly Random _random;

        public ReceipeService_Should()
        {
            _random = new Random();
        }

        //Given_When_Then
        [Fact]
        public async Task GetAllAsync_WithOrderByAndOrderHow_ReturnReceipes()
        {
            //Arange - preparation of data, creation of mock data model objects
            var receipesRepositoryMock = new Mock<IRecipesRepository>();
            var descriptionsRepositoryMock = new Mock<IDescriptionsRepository>();

            // expected input - data that would be provided to the tested method.
            var orderBy = Guid.NewGuid().ToString();
            var orderHow = Guid.NewGuid().ToString();


            var requestFilter = new RecipesFilter
            {
                OrderBy = orderBy,
                OrderHow = orderHow
            };

            //expected output from receipesRepository
            var getReceipesResponse = GenerateReceipes(10);

            //Setup defines what is going to happen when the method will be called
            receipesRepositoryMock
                .Setup(receipesRepository => receipesRepository
                //.GetAll(It.IsAny<RecipesFilter>()))
                .GetAll(requestFilter))
                .ReturnsAsync(getReceipesResponse);


            var expectedResult = new List<Recipe>();

            for (var i = 0; i < getReceipesResponse.Count; i++)
            {
                var recipe = new Recipe
                {
                    Id = getReceipesResponse[i].Id,
                    Name = getReceipesResponse[i].Name,
                    Description = getReceipesResponse[i].Description,
                    Difficulty = getReceipesResponse[i].Difficulty,
                    TimeToComplete = getReceipesResponse[i].TimeToComplete,
                    DateCreated = getReceipesResponse[i].DateCreated
                };

                expectedResult.Add(recipe);
            }

            //sut - system under test
            var sut = new RecipeService(receipesRepositoryMock.Object, descriptionsRepositoryMock.Object);

            //Act - we call the method we want to test

            var result = (await sut.GetAllAsync(requestFilter.OrderBy, requestFilter.OrderHow))
                .ToList();

            //Assert - did the required methods were called, did the intended data were returned

            for (var i = 0; i < expectedResult.Count; i++)
            {
                Assert.Equal(expectedResult[i].Id, result[i].Id);
                Assert.Equal(expectedResult[i].Name, result[i].Name);
                Assert.Equal(expectedResult[i].Description, result[i].Description);
                Assert.Equal(expectedResult[i].Difficulty, result[i].Difficulty);
                Assert.Equal(expectedResult[i].TimeToComplete, result[i].TimeToComplete);
                Assert.Equal(expectedResult[i].DateCreated, result[i].DateCreated);
            }

            receipesRepositoryMock
                .Verify(receipesRepository => receipesRepository
                .GetAll(It.Is<RecipesFilter>(value => value.OrderBy.Equals(requestFilter.OrderBy) && value.OrderHow.Equals(requestFilter.OrderHow))), Times.Once);
        }

        private List<RecipeReadModel> GenerateReceipes(int numberOfReceipes)
        {
            var recipes = new List<RecipeReadModel>();

            for (var i = 0; i < numberOfReceipes; i++)
            {
                var recipe = new RecipeReadModel
                {
                    Id = _random.Next(2, 50),
                    Name = Guid.NewGuid().ToString(),
                    Description = Guid.NewGuid().ToString(),
                    Difficulty = (Difficulty)_random.Next(1, 3),
                    TimeToComplete = TimeSpan.FromMinutes(_random.Next(1, 240)),
                    DateCreated = DateTime.Now
                };

                recipes.Add(recipe);
            }

            return recipes;
        }

        //Given_When_Then
        [Fact]
        public async Task CreateAsync_WithReceipe_ReturnRowsAffected()
        {
            //Arange - preparation of data, creation of mock data model objects
            var receipesRepositoryMock = new Mock<IRecipesRepository>();
            var descriptionsRepositoryMock = new Mock<IDescriptionsRepository>();

            // expected input - data that would be provided to the tested method

            var newRecipe = new Recipe
            {
                Id = _random.Next(2, 50),
                Name = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                Difficulty = (Difficulty)_random.Next(1, 3),
                TimeToComplete = TimeSpan.FromMinutes(_random.Next(1, 240)),
                DateCreated = DateTime.Now
            };

            var newRecipeMain = new RecipeWriteModel
            {
                Id = newRecipe.Id,
                Name = newRecipe.Name,
                Difficulty = newRecipe.Difficulty,
                TimeToComplete = newRecipe.TimeToComplete,
                DateCreated = newRecipe.DateCreated
            };

            var newRecipeDescription = new DescriptionWriteModel
            {
                RecipeId = newRecipe.Id,
                Description = newRecipe.Description
            };

            //expected output from receipesRepository
            //var recipeRowsAffected = 1;

            //expected output from descriptionsRepository
            //var descriptionRowsAffected = 1;

            //Setup defines what is going to happen when the method will be called
            receipesRepositoryMock
                .Setup(recipesRepository => recipesRepository
                .SaveAsync(newRecipeMain))
                .ReturnsAsync(It.IsAny<int>());

            descriptionsRepositoryMock
                .Setup(descriptionsRepository => descriptionsRepository
                .SaveAsync(newRecipeDescription))
                .ReturnsAsync(It.IsAny<int>());

            var expectedResult = It.IsAny<int>();

            //sut - system under test
            var sut = new RecipeService(receipesRepositoryMock.Object, descriptionsRepositoryMock.Object);

            //Act - we call the method we want to test

            var result = await sut.CreateAsync(newRecipe);

            //Assert - did the required methods were called, did the intended data were returned

            Assert.Equal(expectedResult, result);

            receipesRepositoryMock
                .Verify(recipesRepository => recipesRepository
                .SaveAsync(It.Is<RecipeWriteModel>(value => value.Id.Equals(newRecipeMain.Id) && value.Name.Equals(newRecipeMain.Name) && value.Difficulty.Equals(newRecipeMain.Difficulty) && value.DateCreated.Equals(newRecipeMain.DateCreated) && value.TimeToComplete.Equals(newRecipeMain.TimeToComplete))), Times.Once);

            descriptionsRepositoryMock
                .Verify(descriptionsRepository => descriptionsRepository
                .SaveAsync(It.Is<DescriptionWriteModel>(value => value.RecipeId.Equals(newRecipeDescription.RecipeId) && value.Description.Equals(newRecipeDescription.Description))), Times.Once);

        }

        [Fact]
        public async Task EditAsync_WithNewValues_ReturnRowsAffected()
        {
            //Arange - preparation of data, creation of mock data model objects
            var receipesRepositoryMock = new Mock<IRecipesRepository>();
            var descriptionsRepositoryMock = new Mock<IDescriptionsRepository>();

            // expected input - data that would be provided to the tested method

            var editedId = new Random().Next(2, 50);
            var newName = Guid.NewGuid().ToString();
            var newDescription = Guid.NewGuid().ToString();

            //expected output from receipesRepository
            //var recipeRowsAffected = 1;

            //expected output from descriptionsRepository
            //var descriptionRowsAffected = 1;

            //Setup defines what is going to happen when the method will be called
            receipesRepositoryMock
                .Setup(recipesRepository => recipesRepository
                .EditNameAsync(editedId, newName))
                .ReturnsAsync(It.IsAny<int>());

            descriptionsRepositoryMock
                .Setup(descriptionsRepository => descriptionsRepository
                .EditDescriptionAsync(editedId, newDescription))
                .ReturnsAsync(It.IsAny<int>());

            var expectedResult = It.IsAny<int>();

            //sut - system under test
            var sut = new RecipeService(receipesRepositoryMock.Object, descriptionsRepositoryMock.Object);

            //Act - we call the method we want to test

            var result = await sut.EditAsync(editedId, newName, newDescription);

            //Assert - did the required methods were called, did the intended data were returned

            Assert.Equal(expectedResult, result);

            receipesRepositoryMock
                .Verify(recipesRepository => recipesRepository
                .EditNameAsync(editedId, newName), Times.Once);

            descriptionsRepositoryMock
                .Verify(descriptionsRepository => descriptionsRepository
                .EditDescriptionAsync(editedId, newDescription), Times.Once);

        }

        [Fact]
        public async Task DeleteByIdAsync_WithProvidedId_ReturnRowsAffected()
        {
            //Arange - preparation of data, creation of mock data model objects
            var receipesRepositoryMock = new Mock<IRecipesRepository>();
            var descriptionsRepositoryMock = new Mock<IDescriptionsRepository>();

            // expected input - data that would be provided to the tested method

            var deletedId = new Random().Next(2, 50);

            //Setup defines what is going to happen when the method will be called
            receipesRepositoryMock
                .Setup(recipesRepository => recipesRepository
                .DeleteByIdAsync(deletedId))
                .ReturnsAsync(It.IsAny<int>());

            descriptionsRepositoryMock
                .Setup(descriptionsRepository => descriptionsRepository
                .DeleteByIdAsync(deletedId))
                .ReturnsAsync(It.IsAny<int>());

            var expectedResult = It.IsAny<int>();

            //sut - system under test
            var sut = new RecipeService(receipesRepositoryMock.Object, descriptionsRepositoryMock.Object);

            //Act - we call the method we want to test

            var result = await sut.DeleteByIdAsync(deletedId);

            //Assert - did the required methods were called, did the intended data were returned

            Assert.Equal(expectedResult, result);

            receipesRepositoryMock
                .Verify(recipesRepository => recipesRepository
                .DeleteByIdAsync(deletedId), Times.Once);

            descriptionsRepositoryMock
                .Verify(descriptionsRepository => descriptionsRepository
                .DeleteByIdAsync(deletedId), Times.Once);

        }

        [Fact]
        public async Task DeleteAllAsync_WhenMethodCalled_ReturnRowsAffected()
        {
            //Arange - preparation of data, creation of mock data model objects
            var receipesRepositoryMock = new Mock<IRecipesRepository>();
            var descriptionsRepositoryMock = new Mock<IDescriptionsRepository>();

            // expected input - data that would be provided to the tested method

            var deletedId = new Random().Next(2, 1023);

            //Setup defines what is going to happen when the method will be called
            receipesRepositoryMock
                .Setup(recipesRepository => recipesRepository
                .DeleteAllAsync())
                .ReturnsAsync(It.IsAny<int>());

            descriptionsRepositoryMock
                .Setup(descriptionsRepository => descriptionsRepository
                .DeleteAllAsync())
                .ReturnsAsync(It.IsAny<int>());

            var expectedResult = It.IsAny<int>();

            //sut - system under test
            var sut = new RecipeService(receipesRepositoryMock.Object, descriptionsRepositoryMock.Object);

            //Act - we call the method we want to test

            var result = await sut.DeleteAllAsync();

            //Assert - did the required methods were called, did the intended data were returned

            Assert.Equal(expectedResult, result);

            receipesRepositoryMock
                .Verify(recipesRepository => recipesRepository
                .DeleteAllAsync(), Times.Once);

            descriptionsRepositoryMock
                .Verify(descriptionsRepository => descriptionsRepository
                .DeleteAllAsync(), Times.Once);

        }
    }


}
