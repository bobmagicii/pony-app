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

		///////////////////////////////////////////////////////////////////////
		// constants //////////////////////////////////////////////////////////

		/* action constants.
		 * these define the different actions a pony can take during its time
		 * on your desktop. */

		public const int TROT = 1;
		public const int STAND = 2;

		public static int[] ValidActions = new int[] {
			Pony.TROT,
			Pony.STAND
		};

		public static int[] ValidActiveActions = new int[] {
			Pony.TROT
		};

		public static int[] ValidPassiveActions = new int[] {
			Pony.STAND
		};

		/* direction constants.
		 * durr left and durr right. made them 1 and -1 so i could use them
		 * in maths to flip flop. flipflopper. */

		public const int RIGHT = 1;
		public const int LEFT = -1;
		public static int[] ValidDirections = new int[] {
			Pony.LEFT,
			Pony.RIGHT
		};

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

		// she really loves you.
		public const int BE_CLINGY = 3;

		///////////////////////////////////////////////////////////////////////
		// instance properties ////////////////////////////////////////////////

		// pony properties
		private String Name;
		private int Mode;
		private int Action = 0;
		private int Direction = 0;

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
			this.Name = name;
			this.Mode = Pony.BE_FREE;

			this.ChoiceTimer = null;
			this.WindowTimer = null;
			this.ClingTimer = null;

			this.Window = new PonyWindow(this);
			this.RNG = new Random();

			this.ChooseWhatDo();
		}

		public int GetMode() {
			return this.Mode;
		}

		public void SetMode(int mode) {
			this.Mode = mode;
		}

		public int GetDirection() {
			return this.Direction;
		}

		public void SetDirection(int dir) {
			this.Direction = dir;
		}

		public int GetAction() {
			return this.Action;
		}

		public void SetAction(int act) {
			this.Action = act;
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
				choice = this.DecideFromPassiveActions();
			}

			// let the pony do whatever she wants if we are allowing here to
			// frolic free.
			else if((this.Mode & Pony.BE_FREE) == Pony.BE_FREE) {
				choice = this.DecideByPersonality();
			}

			// else fallback to being free if unknown mode is unknown.
			else {
				choice = this.DecideByPersonality();
			}

			this.TellWhatDo(choice[0],choice[1]);
		}

		public void TellWhatDo(int action, int direction, bool devoted) {

			if(devoted) {
				// she is devoted to doing this and will stop making decisions
				// for herself.
				this.ChoiceTimer.Stop();
			} else {
				// after she does this she is free to make other choices on
				// her own.
				this.ResetChoiceTimer();
			}

			this.TellWhatDo(action,direction);
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

		/* int ChooseDirection(void);
		 * choose from any of the valid random directions that have been defined
		 * for the pony. */

		private int ChooseDirection() {
			return Pony.ValidDirections[this.RNG.Next(0,Pony.ValidDirections.Length)];
		}

		/* int ChooseAction(void);
		 * choose from any of the valid actions that have been defined for the
		 * pony. */

		private int ChooseAction() {
			return Pony.ValidActions[this.RNG.Next(0,Pony.ValidActions.Length)];
		}

		private int ChooseActiveAction() {
			return Pony.ValidActiveActions[this.RNG.Next(0,Pony.ValidActiveActions.Length)];
		}

		private int ChoosePassiveAction() {
			return Pony.ValidPassiveActions[this.RNG.Next(0,Pony.ValidPassiveActions.Length)];
		}

		/* int-array DecideFromPassiveActions(void);
		 * have the pony choose something she can do that does not involve
		 * running around everywhere. */

		private int[] DecideFromPassiveActions() {
			int[] choice = new int[2];

			Trace.WriteLine("// pony knows you would like her to be still");

			choice[0] = this.ChoosePassiveAction();
			choice[1] = this.ChooseDirection();
			return choice;
		}

		/* int-array DecideByPersonality(void);
		 * allow the pony to choose for herself what her next actions will be.
		 * eventually when multiple ponies are available through config files
		 * this will handle the different personalites */

		private int[] DecideByPersonality() {
			bool undecided = false;
			int action = 0;
			int direction = 0;

			do {

				action = this.ChooseAction();
				direction = this.ChooseDirection();
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

		/* void PauseChoiceEngine(void);
		 */

		public void PauseChoiceEngine() {
			if(this.ChoiceTimer.IsEnabled) {
				Trace.WriteLine("<< pony holds off on making any more choices for now");
				this.TellWhatDo(Pony.STAND, this.Direction, true);
			}
		}

		/* void ResumeChoiceEngine(void);
		 */

		public void ResumeChoiceEngine() {

			// if the pony is doing something active while told to be still
			// she might still be performing an action to prepare. we don not
			// want to distract her. (stopping things like mousein mouseout
			// from restarting the choice engine while on a mission)
			if(this.IsActionActive() && this.Mode == Pony.BE_STILL)
				return;

			// if she is being clingy then she is too busy to get distracted
			// and wander around.
			if(this.Mode == Pony.BE_CLINGY)
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
			if(this.Mode == Pony.BE_CLINGY) {
				this.ClingStop();
			} else {
				this.ClingStart();
			}
		}

		private void ClingStart() {
			this.StopAction();
			this.PauseChoiceEngine();

			this.Mode = Pony.BE_CLINGY;
			this.ClingTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle,this.Window.Dispatcher);
			this.ClingTimer.Interval = TimeSpan.FromMilliseconds(100);
			this.ClingTimer.Tick += new EventHandler(this.ClingTick);
			this.ClingTimer.Start();
		}

		private void ClingStop() {
			this.Mode = Pony.BE_FREE;

			this.ClingTimer.Stop();
			this.ClingTimer = null;

			this.ResumeChoiceEngine();
		}

		private void ClingTick(Object sender, EventArgs e) {

			double dist = ((this.Window.Left + (this.Window.Width / 2)) - System.Windows.Forms.Control.MousePosition.X);

			if(dist < 0 && Math.Abs(dist) >= this.Window.Width) {
				this.TellWhatDo(Pony.TROT,Pony.RIGHT);
			} else if(dist < 0) {
				this.TellWhatDo(Pony.STAND,Pony.RIGHT);
			}

			if(dist > 0 && Math.Abs(dist) >= this.Window.Width) {
				this.TellWhatDo(Pony.TROT,Pony.LEFT);
			} else if(dist > 0) {
				this.TellWhatDo(Pony.STAND, Pony.LEFT);
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
			int direction = this.Direction;

			// figure out if she has wallfaced and needs to change directions.
			if(this.Direction == Pony.RIGHT) {
				if(((this.Window.Left + this.Window.Width) + 3) >= SystemParameters.VirtualScreenWidth) {
					Trace.WriteLine(">> pony hit the right wall");
					direction = Pony.LEFT;
				}
			}
			
			else if(this.Direction == Pony.LEFT) {
				if((this.Window.Left - 3) <= 0) {
					Trace.WriteLine(">> pony hit the left wall");
					direction = Pony.RIGHT;
				}
			}

			// update the window position.
			this.Window.Left += (3 * direction);

			if(direction != this.Direction) {
				// if she was trotting even though she had been told to stand
				// still, now that she has hit the wall just stand there and
				// look pretty.
				if(this.Mode == Pony.BE_STILL) {
					this.TellWhatDo(Pony.STAND, this.Direction, false);
				}
				
				// else abotu face and keep going like a boss.
				else {
					this.TellWhatDo(Pony.TROT, direction);
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

			// let any old images go to get garbage collected.
			if(this.Image != null)
				this.Image.Free();

			// open the new image.
			this.Image = new PonyImage(this.Name,action,direction);

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
