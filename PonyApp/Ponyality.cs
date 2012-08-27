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
		public int Laziness {
			get { return this._Laziness; }
			set { this._Laziness = this.Range100(50+value); }
		} private int _Laziness;

		/// <summary>
		/// scale 0 to 100, how quirky is this pony? this will modify how often
		/// this pony's "special" things are likely to happen when she decides
		/// to perform one.
		/// </summary>
		public int Quirkiness {
			get { return this._Quirkiness; }
			set { this._Quirkiness = this.Range100(10+value); }
		} private int _Quirkiness;

		/// <summary>
		/// scale 0 to 100, how spazzy is this pony? this will modify how often
		/// she does things like change directions.
		/// </summary>
		public int Spazziness {
			get { return this._Spazziness; }
			set { this._Spazziness = this.Range100(25+value); }
		} private int _Spazziness;

		/// <summary>
		/// scale 0 to 100, how long can this pony keep going and going and
		/// going? modifies things like how likely to do something active
		/// after already having done something active.
		/// </summary>
		public int Stamina {
			get { return this._Stamina; }
			set { this._Stamina = this.Range100(15+value); }
		} private int _Stamina;

		///////////////////////////////////////////////////////////////////////
		///////////////////////////////////////////////////////////////////////

		/// <summary>
		/// construct a ponyality with the default values.
		/// </summary>
		public Ponyality() {
			this.Laziness =
			this.Quirkiness =
			this.Spazziness =
			this.Stamina =
			-1;
		}

		/// <summary>
		/// provide a range check for 0 to 100.
		/// </summary>
		public int Range100(int value) {
			if(value < 0) return 0;
			else if(value > 100) return 100;
			else return value;
		}

		/// <summary>
		/// provide a string representation for output.
		/// </summary>
		override public string ToString() {
			return String.Format(
				"Lazy: {0}, Quirky: {1}, Spazzy: {2}, Stamina: {3}",
				this.Laziness,
				this.Quirkiness,
				this.Spazziness,
				this.Stamina
			);
		}

	}
}
