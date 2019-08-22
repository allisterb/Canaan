using System;
using System.Collections.Generic;
using System.Text;

namespace Canaan
{
    public class Article
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Category { get; set; }

        public Uri Uri { get; set; }

        public string Source { get; set; }

        public string Text { get; set; }

        public string Lede { get; set; }

        public int? WordCount { get; set; }
    }
}
