using System;
using System.Collections.Generic;
using System.Text;

namespace Canaan
{
    public class Article
    {
        public string Id { get; set; }

        public int Position { get; set;
        }
        public string Title { get; set; }

        public string Category { get; set; }

        public string Description { get; set; }

        public DateTime? DatePublished { get; set; }

        public Uri Uri { get; set; }

        public string Author { get; set; }

        public string Source { get; set; }

        public string Aggregator { get; set; }

        public string FullText { get; set; }

        public string Lede { get; set; }

        public int? WordCount { get; set; }
    }
}
