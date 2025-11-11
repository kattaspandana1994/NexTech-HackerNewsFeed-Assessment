using NexTech.HackerNews.Feed.Application.DTOs;

namespace NexTech.HackerNews.Feed.Api.Tests
{
    public static class StorySeeder
    {
        public static List<HackerNewsStory> GetSampleStories(int count = 3)
        {
            var stories = new List<HackerNewsStory>();
            for (int i = 1; i <= count; i++)
            {
                stories.Add(new HackerNewsStory
                {
                    Id = i,
                    Title = $"Story {i}",
                    Url = $"http://story{i}.com"
                });
            }
            return stories;
        }

        public static HackerNewsStory GetSingleStory(int id = 1, string title = "Default Story")
        {
            return new HackerNewsStory
            {
                Id = id,
                Title = title,
                Url = $"http://story{id}.com"
            };
        }

        public static List<HackerNewsStory> GetEmptyStories() => new List<HackerNewsStory>();
    }
}
