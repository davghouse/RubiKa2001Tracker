using System;
using System.Collections.Generic;

namespace RubiKa2001Tracker.ForumScraper
{
    public class Thread
    {
        public int ID { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public int PostCount { get; set; }
        public int PageCount => (int)Math.Ceiling(PostCount / 20m);
        public IReadOnlyList<Post> Posts { get; set; }

        public override string ToString()
            => Url;
    }
}
