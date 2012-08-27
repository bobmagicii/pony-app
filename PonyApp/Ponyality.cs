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
		private int _Laziness;
		public  int  Laziness {
			get { return this._Laziness; }
			set {
				if(!this.Range100(value)) this._Laziness = 42;
				else this._Laziness = value;
			}
		}

		/// <summary>
		/// scale 0 to 100, how quirky is this pony? this will modify how often
		/// this pony's "special" things are likely to happen when she decides
		/// to perform one.
		/// </summary>
		private int _Quirkiness;
		public  int  Quirkiness {
			get { return this._Quirkiness; }
			set {
				if(!this.Range100(value)) this._Quirkiness = 12;
				else this._Quirkiness = value;
			}
		}

		/// <summary>
		/// scale 0 to 100, how spazzy is this pony? this will modify how often
		/// she does things like change directions.
		/// </summary>
		private int _Spazziness;
		public  int  Spazziness {
			get { return this._Spazziness; }
			set {
				if(!this.Range100(value)) this._Spazziness = 30;
				else this._Spazziness = value;
			}
		}

		/// <summary>
		/// scale 0 to 100, how long can this pony keep going and going and
		/// going? modifies things like how likely to do something active
		/// after already having done something active.
		/// </summary>
		private int _Stamina;
		public  int  Stamina {
			get { return this._Stamina; }
			set {
				if(!this.Range100(value)) this._Stamina = 30;
				else this._Stamina = value;
			}
		}

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
		public bool Range100(int value) {
			return (value >= 0 && value <= 100);
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
