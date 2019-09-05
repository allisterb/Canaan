using System;
using System.Collections.Generic;
using System.Text;

namespace Canaan
{
    public interface INewsItem : IItem
    {
        string AuthorOrUser { get; }

        string Text { get; set; }
    }
}
