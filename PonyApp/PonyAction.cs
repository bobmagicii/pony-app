using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PonyApp {
	public enum PonyAction {
		None = 0,  // an unknown for testing against.
		Trot,      // casual jogging across the screen.
		Stand,     // stay in one place, mostly idle, looking pretty.
		Action1,   // an active ponyality action.
		Action2,   // an active ponyality action.
		Passive1,  // a passive ponyality action.
		Gallop,    // sprinting across the screen.
		Teleport,  // teleporting somewhere else.
		Teleport2  // teleporting in animation. (not directly callable)
	}

	public enum PonyActionActive {
		None = 0,
		Trot = PonyAction.Trot,
		Action1 = PonyAction.Action1,
		Gallop = PonyAction.Gallop,
		Teleport = PonyAction.Teleport
	}

	public enum PonyActionPassive {
		None = 0,
		Stand = PonyAction.Stand,
		Passive1 = PonyAction.Passive1
	}
}
