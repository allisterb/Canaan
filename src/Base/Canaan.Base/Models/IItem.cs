using System;
using System.Collections.Generic;
using System.Text;

namespace Canaan
{
    public interface IItem
    {
        string IId { get; }

        string Source { get; }

        DateTime Date { get; }
    }
}
