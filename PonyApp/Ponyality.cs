using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PonyApp {
	public class Ponyality {

		/// <summary>
		/// scale 0 to 100, how lazy is pony? this will modify behaviours like
		/// how likely she is to do something active after being passive. if
		/// she is really lazy she will *probably* not be very energetic.
		/// </summary>
		public int Laziness { get; set; }

		/// <summary>
		/// scale 0 to 100, how quirky is this pony? this will modify how often
		/// this pony's "special" things are likely to happen when she decides
		/// to perform one.
		/// </summary>
		public int Quirkiness { get; set; }

		/// <summary>
		/// scale 0 to 100, how spazzy is this pony? this will modify how often
		/// she does things like change directions.
		/// </summary>
		public int Spazziness { get; set; }

		/// <summary>
		/// scale 0 to 100, how long can this pony keep going and going and
		/// going? modifies things like how likely to do something active
		/// after already having done something active.
		/// </summary>
		public int Stamina { get; set; }

		public Ponyality() {
			this.Laziness = 42;
			this.Quirkiness = 12;
			this.Spazziness = 30;
			this.Stamina = 32;
		}

	}
}
