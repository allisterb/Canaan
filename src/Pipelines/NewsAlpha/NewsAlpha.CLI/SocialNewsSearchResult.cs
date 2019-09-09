using System;
using System.Collections.Generic;
using System.Text;

namespace NewsAlpha
{
    public class SocialNewsSearchResult
    {
        public string Id { get; set; }

        public string User { get; set; }

        public string Text { get; set; }

        public DateTime DatePublished { get; set; }

        public bool HasIdentityHate { get; set; }

        public string[] Links { get; set; }
    }
}
