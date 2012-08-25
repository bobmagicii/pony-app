using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PonyApp
{
    public enum PonyAction
    {
        None = 0,
        Trot = 1,
        Stand = 2
    }

	public enum PonyActionActive
	{
		None = 0,
		Trot = PonyAction.Trot
	}

	public enum PonyActionPassive {
		None = 0,
		Trot = PonyAction.Stand
	}
}
