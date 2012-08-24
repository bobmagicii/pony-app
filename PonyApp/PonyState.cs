using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PonyApp
{
    public class PonyState
    {
        public PonyAction Action { get; set; }
        public PonyDirection Direction { get; set; }
        public PonyMode Mode { get; set; }
    }
}
