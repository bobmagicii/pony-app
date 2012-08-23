using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Timers;
using WpfAnimatedGif;

namespace PonyApp {

	public class Pony {

		///////////////////////////////////////////////////////////////////////
		// constants //////////////////////////////////////////////////////////

		/* action constants.
		 * these define the different actions a pony can take during its time
		 * on your desktop. */

		public const int TROT = 1;
		public const int STAND = 2;

		/* direction constants.
		 * durr left and durr right. */

		public const int RIGHT = 1;
		public const int LEFT = 2;

		/* pony modes.
		 * different modes for requesting she behaves decent when visiting
		 * your parents for the first time.
		 * 
		 * note: stagger these so they can be bitmathed, i might want to add a
		 * flag to represent chattiness or something later. */

		// in this mode she is free to do whatever the hell she wants whenever
		// she wants. you can't hold a pony back.
		public const int BE_FREE = 1;

		// ask her to kind of keep to herself and stay in one place for some
		// time.
		public const int BE_STILL = 2;

		///////////////////////////////////////////////////////////////////////
		// instance properties ////////////////////////////////////////////////

		// pony properties
		private String Name;
		private int Action;
		private int Direction;
		private int Mode;

		// physical properties
		private PonyWindow Window;
		private BitmapImage Image;
		private Random RNG;

		// this timer is for moving the window around. unfortunately if the pc
		// is too busy or i set it too fast, it slow down the gif rendering
		// quite a bit as it shares the ui thread. problem is that
		// system.timers.timer didn't have permissions to update the object
		// owned by another thread.
		private DispatcherTimer WindowTimer;

		// this timer is for when the pony decides it should do something else.
		// time till bordem basically.
		private DispatcherTimer ChoiceTimer;

		/* constructor Pony(string name);
		 * constructs. give it a pony name. */

		public Pony(String name) {
			this.Name = name;
			this.Action = Pony.TROT;
			this.Direction = 0;
			this.Mode = Pony.BE_FREE;

			this.ChoiceTimer = null;
			this.WindowTimer = null;

			this.Window = new PonyWindow(this);
			this.RNG = new Random();
		}

		///////////////////////////////////////////////////////////////////////
		// decision methods ///////////////////////////////////////////////////

		/* void ResetChoiceTimer(void);
		 * reset the choice timer with a value that indicates how the lively
		 * the pony feels and how often she feels like deciding to do
		 * something */

		public void ResetChoiceTimer() {
			// ponies in motion tend to stay in motion, while ponies at
			// rest tend to stay at rest.
			if(this.IsActionActive()) {
				Trace.WriteLine("// pony feels energetic.");
				this.ChoiceTimer.Interval = TimeSpan.FromSeconds(this.RNG.Next(3, 6));
			} else {
				Trace.WriteLine("// pony feels lazy.");
				this.ChoiceTimer.Interval = TimeSpan.FromSeconds(this.RNG.Next(12, 20));
			}

			this.ChoiceTimer.Start();
		}

		/* void ChooseWhatDo(Object sender, EventArgs e);
		 * Version of ChooseWhatDo for the ChoiceTimer callback. */

		public void ChooseWhatDo(Object sender, EventArgs e) {
			this.ChoiceTimer.Stop();
			this.ChooseWhatDo();
			this.ResetChoiceTimer();
		}

		/* void ChooseWhatDo(void);
		 * do stuff that allows anypony to choose for herself what to do next */

		public void ChooseWhatDo() {
			int[] choice;

			// if we asked the pony to be still, restrict her options.
			if((this.Mode & Pony.BE_STILL) == Pony.BE_STILL) {
				choice = this.ChooseFromPassiveActions();
			}

			// let the pony do whatever she wants if we are allowing here to
			// frolic free.
			else if((this.Mode & Pony.BE_FREE) == Pony.BE_FREE) {
				choice = this.ChooseByPersonality();
			}

			// else fallback to being free if unknown mode is unknown.
			else {
				choice = this.ChooseByPersonality();
			}

			this.TellWhatDo(choice[0],choice[1]);
		}

		/* void TellWhatDo(int action, int direction);
		 * tell this pony what to (even if that means she is telling herself what
		 * to do), and do it. also if this is the first action a pony has made
		 * then it will start the choice timer up. */

		public void TellWhatDo(int action, int direction) {

			Trace.WriteLine(
				">> Action: " + this.Action.ToString() + ", "+
				"Direction: " + this.Direction.ToString()
			);

			// no need to muck with the image and window if we are doing more of the
			// same yeh?
			if(action != this.Action || direction != this.Direction) {
				this.Action = action;
				this.Direction = direction;
				this.StartAction();
			}

			// if this is the first action our pony has done, then we also need to
			// spool the decision engine up.
			if(this.ChoiceTimer == null) {
				this.ChoiceTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle,this.Window.Dispatcher);
				this.ChoiceTimer.Tick += new EventHandler(this.ChooseWhatDo);
				this.ResetChoiceTimer();
			}
		}

		/* int-array ChoosePassiveAction(void);
		 * have the pony choose something she can do that does not involve
		 * running around everywhere. */

		private int[] ChooseFromPassiveActions() {
			int[] choice = new int[2];
			int action = Pony.STAND;
			int direction = this.RNG.Next(1,3);

			// coming soon lol

			choice[0] = action;
			choice[1] = direction;
			return choice;
		}

		/* int-array ChooseByPersonality(void);
		 * allow the pony to choose for herself what her next actions will be.
		 * eventually when multiple ponies are available through config files
		 * this will handle the different personalites */

		private int[] ChooseByPersonality() {
			bool undecided = false;
			int action = 0;
			int direction = 0;

			do {

				action = this.RNG.Next(1, 3);
				direction = this.RNG.Next(1, 3);
				undecided = false;

				// pony generally like be lazy. if she is doing somethng lazy there is a
				// greater chance she will not want to do something active.
				if(!this.IsActionActive() && Pony.IsActionActive(action)) {
					if(this.RNG.Next(1, 101) <= 42) {
						Trace.WriteLine(">> pony feels indecisive about what to do next.");
						undecided = true;
					}
				}

				if(direction != this.Direction) {
					// if pony is doing something active and will continue to do so then there
					// is a higher chance she will not change directions.
					if(this.IsActionActive() && Pony.IsActionActive(action)) {
						if(this.RNG.Next(1, 101) <= 70) {
							Trace.WriteLine(">> pony's momentum carries her through");
							direction = this.Direction;
						}
					}

					// if pony is doing something active and she suddenly stops then this too
					// will have a greater chance of not changing directions.
					if(this.IsActionActive() && !Pony.IsActionActive(action)) {
						if(this.RNG.Next(1, 101) <= 70) {
							Trace.WriteLine(">> pony stops in her tracks");
							direction = this.Direction;
						}
					}
				}

			} while(undecided);

			int[] choice = new int[2];
			choice[0] = action;
			choice[1] = direction;
			return choice;
		}

		///////////////////////////////////////////////////////////////////////
		// action methods /////////////////////////////////////////////////////

		/* void StartAction(void);
		 * stop any current actions, load the image that matches, and begin the
		 * an action sequence. */

		public void StartAction() {

			// stop any current action.
			this.StopAction();

			// load the new image.
			this.LoadImage();

			// place the window just above the task bar so it looks like they
			// be walkin on it. this might be broken on multi-head or non
			// bottom taskbars, nfi. the 10 is an offset that will need to be
			// per pony probably.
			this.Window.Top = SystemParameters.MaximizedPrimaryScreenHeight - this.Window.Height - 10;

			switch(this.Action) {
				case Pony.TROT:
					this.Trot();
					break;

				case Pony.STAND:
					this.Stand();
					break;
			}
		}

		/* void StopAction(void);
		 * stop any timers and whatnot that may be screwing with the window or
		 * image to prepare for whatever we want to do next. */

		public void StopAction() {
			if(this.WindowTimer != null) {
				this.WindowTimer.Stop();
				this.WindowTimer = null;
				// this is me assuming there is a garbage collector in this
				// language and that it is doing its goddamn job :)
			}
		}

		/* void PauseAction(void);
		 */

		public void PauseAction() {
			Trace.WriteLine("<< pony is holding short for you");
			this.TellWhatDo(Pony.STAND,this.Direction);
			this.ChoiceTimer.Stop();
		}

		/* void ResumeAction(void);
		 */

		public void ResumeAction() {
			Trace.WriteLine("<< pony left to her own devices");
			this.ChoiceTimer.Start();
		}

		/*
		 * STANDING
		 * all you have to do is look pretty.
		 */

		private void Stand() {

		}

		/*
		 * TROTTING
		 * walking back, and forth, and back, and forth.
		 */

		private void Trot() {
			this.WindowTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle, this.Window.Dispatcher);
			this.WindowTimer.Interval = TimeSpan.FromMilliseconds(25);
			this.WindowTimer.Tick += new EventHandler(this.TrotTick);
			this.WindowTimer.Start();
		}

		private void TrotTick(Object sender, EventArgs e) {

			// if trotting to the right detect hitting the right edge.
			if(this.Direction == Pony.RIGHT) {
				if(((this.Window.Left + this.Window.Width) + 3) >= SystemParameters.VirtualScreenWidth) {
					Trace.WriteLine("hit the right wall");
					this.Direction = Pony.LEFT;
					this.LoadImage();
				} else {
					this.Window.Left += 3;
				}
			}

			// if trotting to the left detect hitting the left edge.
			if(this.Direction == Pony.LEFT) {
				if(this.Window.Left - 3 <= 0) {
					Trace.WriteLine("hit the left wall");
					this.Direction = Pony.RIGHT;
					this.LoadImage();
				} else {
					this.Window.Left -= 3;
				}
			}

		}

		///////////////////////////////////////////////////////////////////////
		// image management methods ///////////////////////////////////////////

		/* void Pony.LoadImage(void);
		 * this version of the method will load the image that matches the
		 * state of the current Action and Direction properties. */

		private void LoadImage() {
			this.LoadImage(this.Action,this.Direction);
		}

		/* void Pony.LoadImage(int action, int direction);
		 * this version of the method will load the image for the action and
		 * direction it is told to. */

		private void LoadImage(int action,int direction) {
			string uri = null;

			switch(action) {
				case Pony.TROT:
					if(direction == Pony.RIGHT) uri = "pack://application:,,,/Resources/" + this.Name + "/TrotRight.gif";
					if(direction == Pony.LEFT) uri = "pack://application:,,,/Resources/" + this.Name + "/TrotLeft.gif";
					break;

				case Pony.STAND:
					if(direction == Pony.RIGHT) uri = "pack://application:,,,/Resources/" + this.Name + "/StandRight.gif";
					if(direction == Pony.LEFT) uri = "pack://application:,,,/Resources/" + this.Name + "/StandLeft.gif";
					break;

			}

			// here i assume garbage collection is doing its job again.
			this.Image = new BitmapImage(new Uri(uri));

			// update the window.
			this.Window.Width = this.Image.Width;
			this.Window.Height = this.Image.Height;
			ImageBehavior.SetAnimatedSource(this.Window.Image, this.Image);
		}

		/////////////////////////////////////////////////////////////////////////////
		// utility methods //////////////////////////////////////////////////////////

		/* public Pony.IsActionActive(void);
		 * returns if the current action the pony is doing is active or not (by means
		 * of the static version below this) */

		public bool IsActionActive() {
			return Pony.IsActionActive(this.Action);
		}

		/* static Pony.IsActionActive(int action);
		 * given an action type, it will return a boolean true if this action
		 * is considered active, or false if it is considered lazy. */

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
