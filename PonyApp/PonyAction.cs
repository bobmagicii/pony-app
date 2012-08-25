using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PonyApp
{
    public enum PonyAction
    {
        None    = 0, // an unknown for testing against.
        Trot    = 1, // casual jogging 
        Stand   = 2, // stay in one place, mostly idle, looking pretty.
		Action1 = 3  // active personality action, something unique that pony does.
    }

	public enum PonyActionActive
	{
		None = 0,
		Trot = PonyAction.Trot,
		Action1 = PonyAction.Action1

	}

	public enum PonyActionPassive {
		None = 0,
		Stand = PonyAction.Stand
	}
}
