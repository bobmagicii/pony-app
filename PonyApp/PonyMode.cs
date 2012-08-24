using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PonyApp
{
    [Flags]
    public enum PonyMode
    {
        None = 0,
        Free = 1,
        Still = 2,
        Clingy = 4
    }
}
