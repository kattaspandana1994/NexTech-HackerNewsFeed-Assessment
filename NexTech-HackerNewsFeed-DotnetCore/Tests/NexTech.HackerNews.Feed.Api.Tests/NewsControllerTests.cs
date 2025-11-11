using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NexTech.HackerNews.Feed.Application.DTOs;
using NexTech.HackerNews.Feed.Application.Interfaces;
using NexTech.HackerNews.Feed.Api.Controllers;
using NexTech.HackerNews.Feed.Api.Models;

namespace NexTech.HackerNews.Feed.Tests.Api.Controllers
{
    public class NewsControllerTests
    {
        private readonly Mock<INewsStoryService> _serviceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly NewsController _controller;

        public NewsControllerTests()
        {
            _serviceMock = new Mock<INewsStoryService>();
            _mapperMock = new Mock<IMapper>();
            _controller = new NewsController(_serviceMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task Get_ReturnsOk_WhenStoriesExist()
        {
            // Arrange
            var stories = Enumerable.Range(1, 15)
                .Select(i => new HackerNewsStory { Id = i, Title = $"Title {i}", Url = $"https://url/{i}" })
                .ToList();

            var appResult = new PagedResult<HackerNewsStory> { Items = stories, Count = stories.Count };

            var mapped = new PagedResult<StoryResponse>
            {
                Items = stories.Select(s => new StoryResponse { Title = s.Title, Url = s.Url }).ToList(),
                Count = stories.Count
            };

            _serviceMock
                .Setup(s => s.GetNewestNewStoriesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(appResult);

            _mapperMock
                .Setup(m => m.Map<PagedResult<StoryResponse>>(It.IsAny<PagedResult<HackerNewsStory>>()))
                .Returns(mapped);

            // Act
            var result = await _controller.Get(page: 1, pageSize: 10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var paged = Assert.IsType<PagedResult<StoryResponse>>(okResult.Value);
            Assert.NotNull(paged);
            Assert.Equal(15, paged.Count);
            Assert.True(paged.Items.Count <= 10);
            Assert.Contains(paged.Items, s => s.Title.Contains("Title"));
        }

        [Fact]
        public async Task Get_ReturnsNotFound_WhenNoStoriesExist()
        {
            // Arrange
            var emptyResult = new PagedResult<HackerNewsStory> { Items = new List<HackerNewsStory>(), Count = 0 };

            _serviceMock
                .Setup(s => s.GetNewestNewStoriesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyResult);

            // Act
            var result = await _controller.Get();

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("No stories were found.", notFound.Value);
        }

        [Fact]
        public async Task Get_AppliesPaginationCorrectly()
        {
            // Arrange
            var stories = Enumerable.Range(1, 25)
                .Select(i => new HackerNewsStory { Id = i, Title = $"Title {i}", Url = $"https://url/{i}" })
                .ToList();

            var appResult = new PagedResult<HackerNewsStory> { Items = stories, Count = stories.Count };

            var mapped = new PagedResult<StoryResponse>
            {
                Items = stories.Select(s => new StoryResponse { Title = s.Title, Url = s.Url }).ToList(),
                Count = stories.Count
            };

            _serviceMock
                .Setup(s => s.GetNewestNewStoriesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(appResult);

            _mapperMock
                .Setup(m => m.Map<PagedResult<StoryResponse>>(It.IsAny<PagedResult<HackerNewsStory>>()))
                .Returns(mapped);

            // Act
            var result = await _controller.Get(page: 2, pageSize: 10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var paged = Assert.IsType<PagedResult<StoryResponse>>(okResult.Value);
            Assert.Equal(25, paged.Count);
            Assert.Equal(10, paged.Items.Count);
            Assert.Equal("Title 11", paged.Items.First().Title);
        }

        [Fact]
        public async Task Search_ReturnsOk_WhenStoriesFound()
        {
            // Arrange
            var stories = Enumerable.Range(1, 5)
                .Select(i => new HackerNewsStory { Id = i, Title = $"QueryTitle {i}", Url = $"https://url/{i}" })
                .ToList();

            var appResult = new PagedResult<HackerNewsStory> { Items = stories, Count = stories.Count };

            var mapped = new PagedResult<StoryResponse>
            {
                Items = stories.Select(s => new StoryResponse { Title = s.Title, Url = s.Url }).ToList(),
                Count = stories.Count
            };

            _serviceMock
                .Setup(s => s.SearchStoriesAsync("query", 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(appResult);

            _mapperMock
                .Setup(m => m.Map<PagedResult<StoryResponse>>(It.IsAny<PagedResult<HackerNewsStory>>()))
                .Returns(mapped);

            // Act
            var result = await _controller.Search("query", 1, 10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var paged = Assert.IsType<PagedResult<StoryResponse>>(okResult.Value);
            Assert.NotEmpty(paged.Items);
            Assert.Contains(paged.Items, s => s.Title.Contains("QueryTitle"));
        }

        [Fact]
        public async Task Search_ReturnsBadRequest_WhenQueryIsEmpty()
        {
            // Act
            var result = await _controller.Search("", 1, 10);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Query parameter cannot be empty.", badRequest.Value);
        }

        [Fact]
        public async Task Search_ReturnsNotFound_WhenNoResultsFound()
        {
            // Arrange
            var emptyResult = new PagedResult<HackerNewsStory> { Items = new List<HackerNewsStory>(), Count = 0 };

            _serviceMock
                .Setup(s => s.SearchStoriesAsync("nothing", 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyResult);

            // Act
            var result = await _controller.Search("nothing", 1, 10);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("No stories found matching the search criteria.", notFound.Value);
        }
    }
}