using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PonyApp
{
    public enum PonyAction
    {
        [ActiveAction, PassiveAction]
        None = 0, // an unknown for testing against.
        [ActiveAction]
        Trot    = 1, // casual jogging 
        [PassiveAction]
        Stand = 2, // stay in one place, mostly idle, looking pretty.
        [ActiveAction]
        Action1 = 3  // active personality action, something unique that pony does.
    }
}
