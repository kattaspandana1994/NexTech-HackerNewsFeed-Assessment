namespace NexTech.HackerNews.Feed.Api.Mappings
{
    using AutoMapper;
    using NexTech.HackerNews.Feed.Application.DTOs;
    using NexTech.HackerNews.Feed.Api.Models;

    /// <summary>
    /// Configures mapping between Application DTOs and API Response Models.
    /// </summary>
    public class StoryMapper : Profile
    {
        public StoryMapper()
        {
            // Map individual items
            CreateMap<HackerNewsStory, StoryResponse>();

            // Map paged results directly using generics
            CreateMap(typeof(PagedResult<>), typeof(PagedResult<>))
                .ForMember("Items", opt => opt.MapFrom("Items"));
        }
    } 
}