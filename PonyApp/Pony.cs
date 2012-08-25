using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
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
		public static List<PonyAction> ValidActions { get; private set; }
		public static List<PonyAction> ValidActiveActions { get; private set; }
		public static List<PonyAction> ValidPassiveActions { get; private set; }

		/* direction constants.
		 * durr left and durr right. made them 1 and -1 so i could use them
		 * in maths to flip flop. flipflopper. */

		public static List<PonyDirection> ValidDirections { get; private set; }

		///////////////////////////////////////////////////////////////////////
		// instance properties ////////////////////////////////////////////////

		// pony properties
		private String Name;
		public PonyMode Mode { get; set; }
		public PonyAction Action { get; set; }
		public PonyDirection Direction { get; set; }

		// physical properties
		public PonyWindow Window;
		private PonyImage Image;
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

		// this timer is for clingy mode for how often she checks to see where
		// you are.
		private DispatcherTimer ClingTimer;

		/* constructor Pony(string name);
		 * constructs. give it a pony name. */

		public Pony(String name) {
			Name = name;
			Mode = PonyMode.Free;

			ChoiceTimer = null;
			WindowTimer = null;
			ClingTimer = null;

			Window = new PonyWindow(this);
			RNG = new Random();

			ChooseWhatDo();
		}

		static Pony() {
			ValidActions = new List<PonyAction>() { PonyAction.Trot, PonyAction.Stand };
			ValidActiveActions = new List<PonyAction>() { PonyAction.Trot };
			ValidPassiveActions = new List<PonyAction>() { PonyAction.Stand };
			ValidDirections = new List<PonyDirection>() { PonyDirection.Left, PonyDirection.Right };
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

			if(this.ChoiceTimer.IsEnabled)
				this.ChoiceTimer.Stop();

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

		public void ChooseWhatDo(object sender, EventArgs e) {
			this.ChoiceTimer.Stop();
			this.ChooseWhatDo();
			this.ResetChoiceTimer();
		}

		/* void ChooseWhatDo(void);
		 * do stuff that allows anypony to choose for herself what to do next */

		public void ChooseWhatDo() {
			PonyState choice;

			// if we asked the pony to be still, restrict her options.
			if((this.Mode & PonyMode.Still) == PonyMode.Still) {
				choice = this.DecideFromPassiveActions();
			}

			// let the pony do whatever she wants if we are allowing here to
				// frolic free.
			else if((this.Mode & PonyMode.Free) == PonyMode.Free) {
				choice = this.DecideByPersonality();
			}

			// else fallback to being free if unknown mode is unknown.
			else {
				choice = this.DecideByPersonality();
			}

			this.TellWhatDo(choice.Action, choice.Direction);
		}

		public void TellWhatDo(PonyAction action, PonyDirection direction, bool devoted) {

			if(devoted) {
				// she is devoted to doing this and will stop making decisions
				// for herself.
				this.ChoiceTimer.Stop();
			} else {
				// after she does this she is free to make other choices on
				// her own.
				this.ResetChoiceTimer();
			}

			this.TellWhatDo(action, direction);
		}

		/* void TellWhatDo(int action, int direction);
		 * tell this pony what to (even if that means she is telling herself what
		 * to do), and do it. also if this is the first action a pony has made
		 * then it will start the choice timer up. */

		public void TellWhatDo(PonyAction action, PonyDirection direction) {

			Trace.WriteLine(
				">> Action: " + this.Action.ToString() + ", " +
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
				ChoiceTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle, this.Window.Dispatcher);
				ChoiceTimer.Tick += ChooseWhatDo;
				ResetChoiceTimer();
			}

		}

		/* int ChooseDirection(void);
		 * choose from any of the valid random directions that have been defined
		 * for the pony. */

		private PonyDirection ChooseDirection() {
			return ValidDirections[this.RNG.Next(0, Pony.ValidDirections.Count)];
		}

		/* int ChooseAction(void);
		 * choose from any of the valid actions that have been defined for the
		 * pony. */

		private PonyAction ChooseAction() {
			return ValidActions[this.RNG.Next(0, Pony.ValidActions.Count)];
		}

		private PonyAction ChooseActiveAction() {
			return ValidActiveActions[this.RNG.Next(0, Pony.ValidActiveActions.Count)];
		}

		private PonyAction ChoosePassiveAction() {
			return ValidPassiveActions[this.RNG.Next(0, Pony.ValidPassiveActions.Count)];
		}

		/* int-array DecideFromPassiveActions(void);
		 * have the pony choose something she can do that does not involve
		 * running around everywhere. */

		private PonyState DecideFromPassiveActions() {

			Trace.WriteLine("// pony knows you would like her to be still");

			var choice = new PonyState {
				Action = ChoosePassiveAction(),
				Direction = ChooseDirection()
			};

			return choice;
		}

		/* int-array DecideByPersonality(void);
		 * allow the pony to choose for herself what her next actions will be.
		 * eventually when multiple ponies are available through config files
		 * this will handle the different personalites */

		private PonyState DecideByPersonality() {
			var choice = new PonyState();
			bool undecided = false;

			do {

				choice.Action = this.ChooseAction();
				choice.Direction = this.ChooseDirection();
				undecided = false;

				// pony generally like be lazy. if she is doing somethng lazy there is a
				// greater chance she will not want to do something active.
				if(!this.IsActionActive() && Pony.IsActionActive(choice.Action)) {
					if(this.RNG.Next(1, 101) <= 42) {
						Trace.WriteLine(">> pony feels indecisive about what to do next.");
						undecided = true;
					}
				}

				if(choice.Direction != this.Direction) {
					// if pony is doing something active and will continue to do so then there
					// is a higher chance she will not change directions.
					if(this.IsActionActive() && Pony.IsActionActive(choice.Action)) {
						if(this.RNG.Next(1, 101) <= 70) {
							Trace.WriteLine(">> pony's momentum carries her through");
							choice.Direction = this.Direction;
						}
					}

					// if pony is doing something active and she suddenly stops then this too
					// will have a greater chance of not changing directions.
					if(this.IsActionActive() && !Pony.IsActionActive(choice.Action)) {
						if(this.RNG.Next(1, 101) <= 70) {
							Trace.WriteLine(">> pony stops in her tracks");
							choice.Direction = this.Direction;
						}
					}
				}

			} while(undecided);

			return choice;
		}

		///////////////////////////////////////////////////////////////////////
		// action management methods //////////////////////////////////////////

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

			switch(Action) {
				case PonyAction.Trot:
					this.Trot();
					break;

				case PonyAction.Stand:
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

		/* void PauseChoiceEngine(void);
		 */

		public void PauseChoiceEngine() {
			if(this.ChoiceTimer.IsEnabled) {
				Trace.WriteLine("<< pony holds off on making any more choices for now");
				this.TellWhatDo(PonyAction.Stand, this.Direction, true);
			}
		}

		/* void ResumeChoiceEngine(void);
		 */

		public void ResumeChoiceEngine() {

			// if the pony is doing something active while told to be still
			// she might still be performing an action to prepare. we don not
			// want to distract her. (stopping things like mousein mouseout
			// from restarting the choice engine while on a mission)
			if(this.IsActionActive() && this.Mode == PonyMode.Still)
				return;

			// if she is being clingy then she is too busy to get distracted
			// and wander around.
			if(this.Mode == PonyMode.Clingy)
				return;

			// else it is ok to restart the choice timer.
			if(!this.ChoiceTimer.IsEnabled) {
				Trace.WriteLine("<< pony left to her own devices");
				this.ChoiceTimer.Start();
			}
		}

		///////////////////////////////////////////////////////////////////////
		// actual action methods //////////////////////////////////////////////

		/* CLINGY MODE
		 * omg why u keep follow me
		 */

		public void ClingToggle() {
			if(this.Mode == PonyMode.Clingy) {
				this.ClingStop();
			} else {
				this.ClingStart();
			}
		}

		private void ClingStart() {
			this.StopAction();
			this.PauseChoiceEngine();

			this.Mode = PonyMode.Clingy;
			this.ClingTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle, this.Window.Dispatcher);
			this.ClingTimer.Interval = TimeSpan.FromMilliseconds(100);
			this.ClingTimer.Tick += new EventHandler(this.ClingTick);
			this.ClingTimer.Start();
		}

		private void ClingStop() {
			this.Mode = PonyMode.Free;

			this.ClingTimer.Stop();
			this.ClingTimer = null;

			this.ResumeChoiceEngine();
		}

		private void ClingTick(Object sender, EventArgs e) {

			double dist = ((this.Window.Left + (this.Window.Width / 2)) - System.Windows.Forms.Control.MousePosition.X);

			if(dist < 0 && Math.Abs(dist) >= this.Window.Width) {
				this.TellWhatDo(PonyAction.Trot, PonyDirection.Right);
			} else if(dist < 0) {
				this.TellWhatDo(PonyAction.Stand, PonyDirection.Right);
			}

			if(dist > 0 && Math.Abs(dist) >= this.Window.Width) {
				this.TellWhatDo(PonyAction.Trot, PonyDirection.Left);
			} else if(dist > 0) {
				this.TellWhatDo(PonyAction.Stand, PonyDirection.Left);
			}


			Trace.WriteLine("mouse distance: " + dist);
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
			// inialize the timer which will run the trot animation of the window movement.
			this.WindowTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle, this.Window.Dispatcher);
			this.WindowTimer.Interval = TimeSpan.FromMilliseconds(20);
			this.WindowTimer.Tick += new EventHandler(this.TrotTick);
			this.WindowTimer.Start();
		}

		private void TrotTick(Object sender, EventArgs e) {
			PonyDirection direction = Direction;

			// figure out if she has wallfaced and needs to change directions.
			if(this.Direction == PonyDirection.Right) {
				if(((this.Window.Left + this.Window.Width) + 3) >= SystemParameters.VirtualScreenWidth) {
					Trace.WriteLine(">> pony hit the right wall");
					direction = PonyDirection.Left;
				}
			} else if(this.Direction == PonyDirection.Left) {
				if((this.Window.Left - 3) <= 0) {
					Trace.WriteLine(">> pony hit the left wall");
					direction = PonyDirection.Right;
				}
			}

			// update the window position.
			this.Window.Left += (3 * (int)direction);

			if(direction != this.Direction) {
				// if she was trotting even though she had been told to stand
				// still, now that she has hit the wall just stand there and
				// look pretty.
				if(this.Mode == PonyMode.Still) {
					this.TellWhatDo(PonyAction.Stand, this.Direction, false);
				}

				// else abotu face and keep going like a boss.
				else {
					this.TellWhatDo(PonyAction.Trot, direction);
				}
			}

		}

		///////////////////////////////////////////////////////////////////////
		// image management methods ///////////////////////////////////////////

		/* void Pony.LoadImage(void);
		 * this version of the method will load the image that matches the
		 * state of the current Action and Direction properties. */

		private void LoadImage() {
			this.LoadImage(this.Action, this.Direction);
		}

		/* void Pony.LoadImage(int action, int direction);
		 * this version of the method will load the image for the action and
		 * direction it is told to. */

		private void LoadImage(PonyAction action, PonyDirection direction) {

			// let any old images go to get garbage collected.
			if(this.Image != null)
				this.Image.Free();

			// open the new image.
			this.Image = new PonyImage(this.Name, action, direction);

			// and apply it to the pony window.
			this.Image.ApplyToPonyWindow(this.Window);

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

		public static bool IsActionActive(PonyAction action) {
			switch(action) {
				case PonyAction.Trot:
					return true;
				default:
					return false;
			}
		}

	}

}
