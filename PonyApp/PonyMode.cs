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
        /// <summary>
        /// in this mode she is free to do whatever the hell she wants whenever she wants. you can't hold a pony back.
        /// </summary>
        Free = 1,
        /// <summary>
        /// ask her to kind of keep to herself and stay in one place for some time.
        /// </summary>
        Still = 2,
        /// <summary>
        /// she really loves you.
        /// </summary>
        Clingy = 4
    }
}
