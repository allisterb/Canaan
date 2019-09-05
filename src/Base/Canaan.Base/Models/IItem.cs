using System;
using System.Collections.Generic;
using System.Text;

namespace Canaan
{
    public interface IItem
    {
        string Id { get; }

        DateTime Date { get; }
  
        string Source { get; }
    }
}
