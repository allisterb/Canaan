using System;
using System.Collections.Generic;
using System.Text;

namespace Canaan
{
    public class Post
    {
        public string Id { get; set; }

        public string ThreadId { get; set; }

        public int Position { get; set; }

        public long No { get; set; }

        public string Author { get; set; }

        public string Text { get; set; }

        public DateTime DatePublished { get; set; }

        public string[] Replies { get; set;  }

        public string ReplyTo { get; set; }
    }
}
