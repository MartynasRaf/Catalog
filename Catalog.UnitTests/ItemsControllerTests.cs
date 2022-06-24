using System;
using System.Collections;
using System.Collections.Generic;
using Catalog.Api.Controllers;
using Catalog.Api.Dtos;
using Catalog.Api.Entities;
using Catalog.Api.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Catalog.UnitTests
{

    public class ItemsControllerTests
    {

        private readonly Mock<IItemsRepository> repositoryStub = new();

        private readonly Random rand = new Random();

        [Fact]
        public async void GetItemAsync_WithUnexistingItem_ReturnsNotFound()
        {
            // Arrange
            repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Item)null);

            var controller = new ItemsController(repositoryStub.Object);
            // Act
            var result = await controller.GetItemAsync(Guid.NewGuid());

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async void GetItemAsync_ExistingItem_ReturnsExpectedItem()
        {
            // Arrange
            var expectedItem = CreateRandomItem();

            repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
            .ReturnsAsync(expectedItem);

            var controller = new ItemsController(repositoryStub.Object);
            // Act
            var result = await controller.GetItemAsync(expectedItem.Id);

            // Assert
            result.Value.Should().BeEquivalentTo(expectedItem);
        }

        [Fact]
        public async void GetItemsAsync_WithExistingItems_ReturnsAllItems()
        {
            // Arrange
            var expectedItems = new[] { CreateRandomItem(), CreateRandomItem(), CreateRandomItem() };

            repositoryStub.Setup(repo => repo.GetItemsAsync()).ReturnsAsync(expectedItems);

            var controller = new ItemsController(repositoryStub.Object);
            // Act
            var actualItems = await controller.GetItemsAsync();
            // Assert
            actualItems.Should().BeEquivalentTo(expectedItems);
        }

        [Fact]
        public async void GetItemsAsync_WithMatchingItems_ReturnsMatchingItems()
        {
            // Arrange
            var allItems = new[]
                {
                new Item(){Name="Potion"},
                new Item(){Name="Antidote"},
                new Item(){Name="Hi-Potion"},
                };

            var nameToMatch = "Potion";

            repositoryStub.Setup(repo => repo.GetItemsAsync())
            .ReturnsAsync(allItems);

            var controller = new ItemsController(repositoryStub.Object);
            // Act

            IEnumerable<ItemDto> foundItems = await controller.GetItemsAsync(nameToMatch);
            
            // Assert
            foundItems.Should().OnlyContain(
                item => item.Name == allItems[0].Name || item.Name == allItems[2].Name
            );
        }

        [Fact]
        public async void CreateItemAsync_WithItemToCreate_ReturnsCreatedItem()
        {
            // Arrange
            var itemToCreate = new CreateItemDto(Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            rand.Next(1000));

            var controller = new ItemsController(repositoryStub.Object);
            // Act
            var result = await controller.CreateItemAsync(itemToCreate);

            // Assert
            var createdItem = (result.Result as CreatedAtActionResult).Value as ItemDto;

            itemToCreate.Should().BeEquivalentTo(createdItem,
            options => options.ComparingByMembers<ItemDto>()
            .ExcludingMissingMembers());

            createdItem.Id.Should().NotBeEmpty();
            createdItem.CreatedDate.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        }


        [Fact]
        public async void UpdateItemAsync_WithExistingItem_ReturnsNoContent()
        {
            // Arrange
            var existingItem = CreateRandomItem();

            repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
            .ReturnsAsync(existingItem);


            var itemId = existingItem.Id;
            var itemToUpdate = new UpdateItemDto(
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                existingItem.Price + 3);

            var controller = new ItemsController(repositoryStub.Object);
            // Act
            var result = await controller.UpdateItemAsync(itemId, itemToUpdate);

            // Assert

            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async void DeleteItemAsync_WithExistingItem_ReturnsNoContent()
        {
            // Arrange
            var existingItem = CreateRandomItem();

            repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
            .ReturnsAsync(existingItem);


            var itemId = existingItem.Id;

            var controller = new ItemsController(repositoryStub.Object);
            // Act
            var result = await controller.DeleteItemAsync(itemId);

            // Assert

            result.Should().BeOfType<NoContentResult>();
        }


        private Item CreateRandomItem()
        {
            return new()
            {
                Id = Guid.NewGuid(),
                Name = Guid.NewGuid().ToString(),
                Price = rand.Next(1000),
                CreatedDate = DateTimeOffset.UtcNow
            };
        }
    }
}