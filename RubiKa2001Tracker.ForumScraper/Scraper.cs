using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Io;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RubiKa2001Tracker.ForumScraper
{
    public static class Scraper
    {
        private static readonly Regex _threadUrlIDRegex
            = new Regex("^https://forums-archive.anarchy-online.com/showthread.php\\?(\\d+)$", RegexOptions.Compiled);

        public static async Task Main(string[] scrapeParameters)
            => await ScrapeSubforum(
                fileName: scrapeParameters[0],
                subforumUrl: scrapeParameters[1],
                pageCount: int.Parse(scrapeParameters[2]));

        private static async Task ScrapeSubforum(string fileName, string subforumUrl, int pageCount)
        {
            var threads = await GetThreads(subforumUrl, pageCount);

            for (int t = 0; t < threads.Count; ++t)
            {
                threads[t].Posts = await GetPosts(t, threads);
            }

            await File.WriteAllLinesAsync(fileName, threads
                .SelectMany(t => t.Posts)
                .OrderBy(p => p.Date)
                .Select(p => p.IsOriginalPost
                    ? $"{p.Author} @ {p.Date} ({p.Thread.Title} {p.Thread.ID}):\n{p.Contents}\n"
                    : $"{p.Author} @ {p.Date} ({p.Thread.ID}):\n{p.Contents}\n"));
        }

        private static IBrowsingContext GetBrowsingContext()
        {
            var requester = new DefaultHttpRequester();
            var config = Configuration.Default.With(requester).WithDefaultLoader();

            return BrowsingContext.New(config);
        }

        private static async Task<IReadOnlyList<Thread>> GetThreads(string subforumUrl, int pageCount)
        {
            var threads = new List<Thread>();
            using var browsingContext = GetBrowsingContext();

            for (int p = 1; p <= pageCount; ++p)
            {
                using var subforumDocument = await browsingContext.OpenAsync($"{subforumUrl}/page{p}");

                foreach (var threadItem in subforumDocument.QuerySelectorAll("li[id^=thread]"))
                {
                    var titleAnchor = (IHtmlAnchorElement)threadItem.QuerySelector("a[id^=thread]");
                    int urlTrimIndex = titleAnchor.Href.IndexOf('-', startIndex: titleAnchor.Href.IndexOf('?'));
                    urlTrimIndex = urlTrimIndex > 0 ? urlTrimIndex : titleAnchor.Href.IndexOf('&');
                    string url = titleAnchor.Href.Substring(0, urlTrimIndex);

                    var thread = new Thread
                    {
                        ID = int.Parse(_threadUrlIDRegex.Match(url).Groups[1].Value),
                        Url = url,
                        Title = titleAnchor.Text,
                        PostCount = int.Parse(threadItem.QuerySelector("ul.threadstats a.understate").TextContent.Replace(",", "")) + 1
                    };

                    // Avoid adding "Rooted" threads multiple times.
                    if (!threads.Any(t => t.Url == thread.Url))
                    {
                        threads.Add(thread);
                    }
                }

                Console.WriteLine($"Got threads for page {p}/{pageCount}.");

                await Task.Delay(5000);
            }

            return threads;
        }

        private static async Task<IReadOnlyList<Post>> GetPosts(int threadIndex, IReadOnlyList<Thread> threads)
        {
            var thread = threads[threadIndex];
            var posts = new List<Post>();
            using var browsingContext = GetBrowsingContext();

            for (int p = 1; p <= thread.PageCount; ++p)
            {
                using var threadDocument = await browsingContext.OpenAsync($"{thread.Url}/page{p}");

                foreach (var postItem in threadDocument.QuerySelectorAll("li[id^=post]"))
                {
                    posts.Add(new Post
                    {
                        Thread = thread,
                        Author = postItem.QuerySelector("a.username strong")?.TextContent.TrimStart('_') ?? "Guest",
                        Date = DateTime.Parse(postItem.QuerySelector(".postdate").TextContent.Trim()
                            .Replace("st", null).Replace("nd", null).Replace("rd", null).Replace("th", null)),
                        Contents = postItem.QuerySelector("[id^=post_message] .postcontent").TextContent.Trim()
                    });
                }

                await Task.Delay(2000);
            }

            Console.WriteLine($"Got {posts.Count}/{thread.PostCount} posts for thread {threadIndex + 1}/{threads.Count} \"{thread.Title}\".");

            return posts;
        }
    }
}
