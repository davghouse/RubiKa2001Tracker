using System;
using System.Linq;

namespace RubiKa2001Tracker.ForumScraper
{
    public class Post
    {
        public Thread Thread { get; set; }
        public string Author { get; set; }
        public DateTime Date { get; set; }
        public string Contents { get; set; }
        public bool IsOriginalPost => this == Thread.Posts.First();

        public override string ToString()
            => $"{Author} @ {Date} in {Thread.ID}";
    }
}
