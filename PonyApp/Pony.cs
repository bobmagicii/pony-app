using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PonyApp {

	public class Pony {

		// actions.
		public const int TROT = 1;
		public const int STAND = 2;

		// directions.
		public const int RIGHT = 1;
		public const int LEFT = 2;

		public static bool IsActionActive(int action) {
			switch(action) {
				case Pony.TROT:
					return true;
				default:
					return false;
			}
		}
	}

}
