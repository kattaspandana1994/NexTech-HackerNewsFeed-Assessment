namespace NexTech.HackerNews.Feed.Api.Controllers
{
    using AutoMapper;
    using Microsoft.AspNetCore.Mvc;
    using NexTech.HackerNews.Feed.Application.DTOs;
    using NexTech.HackerNews.Feed.Application.Interfaces;
    using NexTech.HackerNews.Feed.Api.Models;

    /// <summary>
    /// Controller for HackerNews stories retrieval and search operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class NewsController : ControllerBase
    {
        private readonly INewsStoryService _newsStoryService;
        private readonly IMapper _mapper;

        public NewsController(INewsStoryService newsStoryService, IMapper mapper)
        {
            _newsStoryService = newsStoryService;
            _mapper = mapper;
        }

        /// <summary>
        /// Gets the newest HackerNews stories with pagination.
        /// </summary>
        /// <param name="page">Page number (1-based).</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <returns>Paged list of stories.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<StoryResponse>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PagedResult<StoryResponse>>> Get(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            // Fetch paged stories from service
            var appResult = await _newsStoryService.GetNewestNewStoriesAsync(page * pageSize, cancellationToken);

            if (appResult == null || appResult.Count == 0)
                return NotFound("No stories were found.");

            // AutoMapper maps PagedResult<HackerNewsStory> → PagedResult<StoryResponse>
            var apiResult = _mapper.Map<PagedResult<StoryResponse>>(appResult);

            // Slice data for the requested page (service may return larger buffer)
            apiResult.Items = apiResult.Items
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();           

            return Ok(apiResult);
        }

        /// <summary>
        /// Searches stories by title or keyword with pagination.
        /// </summary>
        /// <param name="query">Search keyword (case-insensitive).</param>
        /// <param name="page">Page number (1-based).</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <returns>Paged list of matching stories.</returns>
        [HttpGet("search")]
        [ProducesResponseType(typeof(PagedResult<StoryResponse>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PagedResult<StoryResponse>>> Search(
            [FromQuery] string query,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Query parameter cannot be empty.");

            var appResult = await _newsStoryService.SearchStoriesAsync(query, page, pageSize, cancellationToken);

            if (appResult == null || appResult.Count == 0)
                return NotFound("No stories found matching the search criteria.");

            var apiResult = _mapper.Map<PagedResult<StoryResponse>>(appResult);

            return Ok(apiResult);
        }
    }
}
