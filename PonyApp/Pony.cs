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

		/* direction constants.
		 * durr left and durr right. made them 1 and -1 so i could use them
		 * in maths to flip flop. flipflopper. */

		public static List<PonyDirection> ValidDirections { get; private set; }

		///////////////////////////////////////////////////////////////////////
		// instance properties ////////////////////////////////////////////////

		// pony properties
		public String Name { get; set; }
		private int YOffset { get; set; }
		private List<PonyAction> AvailableActions { get; set; }
		private List<PonyAction> AvailableActiveActions { get; set; }
		private List<PonyAction> AvailablePassiveActions { get; set; }

		public PonyMode Mode { get; set; }
		public PonyAction Action { get; set; }
		public PonyDirection Direction { get; set; }

		// physical properties
		public PonyWindow Window;
		private List<PonyImage> Image;
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

		/// <summary>
		/// create a new pony from the configuration. it will initalize allthe
		/// required properties including building the lists of actions (active
		/// and passive) that this pony instance is capable of doing.
		/// </summary>
		public Pony(PonyConfig config) {
			this.Name = config.Name;
			this.YOffset = config.YOffset;

			// prepare available action lists. 
			this.AvailableActions = config.Actions;
			this.AvailableActiveActions = new List<PonyAction>();
			this.AvailablePassiveActions = new List<PonyAction>();

			// for each available action, classify them as active or passive
			// for our decision making later.
            this.AvailableActions.ForEach(x =>
            {
                if (x.IsActive())
                    AvailableActiveActions.Add(x);

                if (x.IsPassive())
                    AvailablePassiveActions.Add(x);
			});

			Trace.WriteLine(String.Format(
				"== {0} can perform {1} Active and {2} Passive actions",
				this.Name,
				this.AvailableActiveActions.Count,
				this.AvailablePassiveActions.Count
			));

			// prepare action properties.
			this.Mode = PonyMode.Free;
			this.Action = PonyAction.Stand;
			this.Direction = PonyDirection.Right;

			// various timers.
			this.ChoiceTimer = null;
			this.WindowTimer = null;
			this.ClingTimer = null;

			// various utilities
			this.Window = new PonyWindow(this);
			this.RNG = new Random();
			this.Image = new List<PonyImage>();

			// preload action images.
			this.LoadAllImages();

			Trace.WriteLine(String.Format("// {0} says hello",this.Name));

			// go ahead and do something now.
			this.ChooseWhatDo();
		}

		~Pony() {
			Trace.WriteLine(String.Format("== {0} object destructed",this.Name));
		}

		public void Shutdown() {
			if(this.WindowTimer != null) this.WindowTimer.Stop();
			if(this.ChoiceTimer != null) this.ChoiceTimer.Stop();
			if(this.ClingTimer != null) this.ClingTimer.Stop();

			this.WindowTimer = this.ChoiceTimer = this.ClingTimer = null;

			this.Image.ForEach(delegate(PonyImage img){
				img.Free();
				img = null;
			});
			this.Image.Clear();

			this.Window.Pony = null;
			this.Window = null;

			this.AvailableActions = this.AvailableActiveActions = this.AvailablePassiveActions = null;

			Trace.WriteLine(String.Format("// {0} waves goodbye",this.Name));
		}

		///////////////////////////////////////////////////////////////////////
		// decision methods ///////////////////////////////////////////////////

		/// <summary>
		/// reset the choice timer. this means reseed it with a new interval
		/// value with a random number so that her choices feel random. we use
		/// different rng values depending the types of actions she is doing
		/// to hopefully create more natural feeling behaviour.
		/// </summary>
		public void ResetChoiceTimer() {

			if(this.ChoiceTimer.IsEnabled)
				this.ChoiceTimer.Stop();

			// ponies in motion tend to stay in motion, while ponies at
			// rest tend to stay at rest.

			if(this.IsActionActive()) {
				Trace.WriteLine(String.Format("// {0} feels energized",this.Name));

				// personal quirks should be given a little longer to run than generic
				// actions.
				if(this.Action == PonyAction.Action1)
					this.ChoiceTimer.Interval = TimeSpan.FromSeconds(this.RNG.Next(6, 10));
				else
					this.ChoiceTimer.Interval = TimeSpan.FromSeconds(this.RNG.Next(4, 8));

			} else {
				Trace.WriteLine(String.Format("// {0} feels lazy", this.Name));
				this.ChoiceTimer.Interval = TimeSpan.FromSeconds(this.RNG.Next(12, 20));
			}

			this.ChoiceTimer.Start();

		}

		/// <summary>
		/// this version of choose what do responds to the choice engine timer
		/// deciding it is time to do something. it will tell the pony to
		/// choose what to do and reseed the choice timer.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void ChooseWhatDo(object sender, EventArgs e) {

			this.ChoiceTimer.Stop();
			this.ChooseWhatDo();
			this.ResetChoiceTimer();

		}

		/// <summary>
		/// allow the pony to choose what she wants to do next, based on the
		/// current mode she is in.
		/// </summary>
		public void ChooseWhatDo() {
			PonyState choice;

			// if we asked the pony to be still, restrict her options.
			if(this.Mode == PonyMode.Still) {
				choice = this.DecideFromPassiveActions();
			}

			// let the pony do whatever she wants if we are allowing here to
			// frolic free.
			else if(this.Mode == PonyMode.Free) {
				choice = this.DecideByPersonality();
			}

			// else fallback to being free if unknown mode is unknown.
			else {
				choice = this.DecideByPersonality();
			}

			this.TellWhatDo(choice.Action, choice.Direction);
		}

		/// <summary>
		/// tell the pony exactly what to do (even if she is telling herself).
		/// if she is devoted then she will not allow herself to be distracted
		/// until this action is done.
		/// </summary>
		/// <param name="action"></param>
		/// <param name="direction"></param>
		/// <param name="devoted"></param>
		public void TellWhatDo(PonyAction action, PonyDirection direction, bool devoted) {

			if(devoted) {
				// she is devoted to doing this and will stop making decisions
				// for herself.
				this.PauseChoiceEngine();
			} else {
				// after she does this she is free to make other choices on
				// her own.
				this.ResetChoiceTimer();
			}

			this.TellWhatDo(action, direction);
		}

		/// <summary>
		/// tell the pony exactly what to do (even if she is telling herself).
		/// </summary>
		/// <param name="action"></param>
		/// <param name="direction"></param>
		public void TellWhatDo(PonyAction action, PonyDirection direction) {

			// can she do it? that is an important question to ask.
			if(direction == PonyDirection.None || action == PonyAction.None || !this.CanDo(action)) {
				Trace.WriteLine(String.Format(
					"!! {0} cannot {1} {2}",
					this.Name,
					action.ToString(),
					direction.ToString()
				));

				return;
			}

			Trace.WriteLine(String.Format(
				">> {0} will {1} to the {2}",
				this.Name,
				action.ToString(),
				direction.ToString()
			));
		
			// no need to muck with the image and window if we are doing more of the
			// same yeh? also check choicetimer as a means of "is this the first
			// action ever" to make sure the default gets loaded.
			if((action != this.Action || direction != this.Direction) || this.ChoiceTimer == null) {
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

		/// <summary>
		/// can the pony do the action requested?
		/// </summary>
		/// <param name="action"></param>
		public bool CanDo(PonyAction action) {
			return this.AvailableActions.Contains(action);
		}

		/// <summary>
		/// choose which direction to go.
		/// </summary>
		private PonyDirection ChooseDirection() {

			switch(this.RNG.Next(1,3)) {
				case 1: return PonyDirection.Left;
				case 2: return PonyDirection.Right;
				default: return PonyDirection.None;
			}

		}

		/// <summary>
		/// have the pony to choose from any of her available actions.
		/// </summary>
		private PonyAction ChooseAction() {
			return this.AvailableActions[this.RNG.Next(0,this.AvailableActions.Count)];
		}

		/// <summary>
		/// have the pony to choose her action but only from her available
		/// active actions. things that do things.
		/// </summary>
		private PonyAction ChooseActiveAction() {
			return this.AvailableActiveActions[this.RNG.Next(0, this.AvailableActiveActions.Count)];
		}

		/// <summary>
		/// have the pony to choose her action but only from her available
		/// passive actions. things that mostly are still and not annoying.
		/// </summary>
		private PonyAction ChoosePassiveAction() {
			return this.AvailablePassiveActions[this.RNG.Next(0, this.AvailablePassiveActions.Count)];
		}

		/// <summary>
		/// tell the pony to choose her next action and direction but only
		/// from her available passive actions.
		/// </summary>
		private PonyState DecideFromPassiveActions() {

			Trace.WriteLine(String.Format(
				"// {0} knows you would like her to be still",
				this.Name
			));

			var choice = new PonyState {
				Action = ChoosePassiveAction(),
				Direction = ChooseDirection()
			};

			return choice;
		}


		/// <summary>
		/// allow the pony's personality to choose her next action and
		/// direction from any of her available actions. she is a free pony
		/// and you should never tell her otherwise.
		/// 
		/// eventually this will be tweakable via the this.pony file for each
		/// pony to make them all unique to their own respective personalities.
		/// </summary>
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
						Trace.WriteLine(String.Format("// {0} feels indecisive about what to do next.",this.Name));
						undecided = true;
						continue;
					}
				}

				// pony do not like to flaunt their personality quirks, if we selected
				// one of the personality actions then there is a high chance she should
				// be undecided. this should make the quirky actions more special when
				// they do actually happen.
				if(choice.Action == PonyAction.Action1) {
					if(this.RNG.Next(1, 101) <= 80) {
						Trace.WriteLine(String.Format("// {0} feels bashful about her quirkiness.",this.Name));
						undecided = true;
						continue;
					}
				}
				

				// pony may tire easy. if she is doing something active and has chosen to
				// continue to be active, there is a chance she is too tired and will be
				// lazy instead.
				if(this.IsActionActive() && Pony.IsActionActive(choice.Action)) {
					if(this.RNG.Next(1, 101) <= 42) {
						Trace.WriteLine(String.Format(">> {0} needed a rest.",this.Name));
						return this.DecideFromPassiveActions();
					}
				}

				// does the pony want to change directions? pony does not like
				// to change direction too often while performing actions.
				if(choice.Direction != this.Direction) {

					// if pony is doing something active and will continue to do so then there
					// is a higher chance she will not change directions.
					if(this.IsActionActive() && Pony.IsActionActive(choice.Action)) {
						if(this.RNG.Next(1, 101) <= 70) {
							Trace.WriteLine(String.Format(">> {0}'s momentum carries her through",this.Name));
							choice.Direction = this.Direction;
						}
					}

					// if pony is doing something active and she suddenly stops then this too
					// will have a greater chance of not changing directions.
					if(this.IsActionActive() && !Pony.IsActionActive(choice.Action)) {
						if(this.RNG.Next(1, 101) <= 70) {
							Trace.WriteLine(String.Format(">> {0} stops in her tracks",this.Name));
							choice.Direction = this.Direction;
						}
					}

				}

			} while(undecided);

			return choice;
		}

		///////////////////////////////////////////////////////////////////////
		// action management methods //////////////////////////////////////////

		/// <summary>
		/// start a new action. this will stop any current actions, load the
		/// new image for the current action, and then execute the specific
		/// actions that she was to do.
		/// </summary>
		public void StartAction() {

			// stop any current action.
			this.StopAction();

			// load the new image.
			this.LoadImage();

			// place the window just above the task bar so it looks like they
			// be walkin on it. this might be broken on multi-head or non
			// bottom taskbars.
			this.Window.Top = SystemParameters.MaximizedPrimaryScreenHeight - this.Window.Height - this.YOffset;

			switch(Action) {
				case PonyAction.Trot:
					this.Trot();
					break;

				case PonyAction.Stand:
					this.Stand();
					break;
			}
		}

		/// <summary>
		/// stop any timers that may be involved with powering any of the
		/// actions.
		/// </summary>
		public void StopAction() {
			if(this.WindowTimer != null) {
				this.WindowTimer.Stop();
				this.WindowTimer = null;
			}
		}

		/// <summary>
		/// temporarily stop the pony from making her own decisions whenever
		/// she feels like it.
		/// </summary>
		public void PauseChoiceEngine() {
			if(this.ChoiceTimer.IsEnabled) {
				Trace.WriteLine(String.Format("<< {0} holds off on making any more choices for now",this.Name));
				this.ChoiceTimer.Stop();
			}
		}

		/// <summary>
		/// allow the pony to resume making her own decisions whenever she
		/// feels like it.
		/// </summary>
		public void ResumeChoiceEngine() {

			// if the pony is doing something active while told to be still
			// she might still be performing an action to prepare. we do not
			// want to distract her. (stopping things like mousein mouseout
			// from restarting the choice engine while on a mission)
			if(this.Mode == PonyMode.Still && this.IsActionActive())
				return;

			// if she is being clingy then she is too busy to get distracted
			// and wander around.
			if(this.Mode == PonyMode.Clingy)
				return;

			// else it is ok to restart the choice timer.
			if(!this.ChoiceTimer.IsEnabled) {
				Trace.WriteLine(String.Format("<< {0} left to her own devices",this.Name));
				this.ChoiceTimer.Start();
			}
		}

		///////////////////////////////////////////////////////////////////////
		// PonyMode.Clingy ////////////////////////////////////////////////////

		/// <summary>
		/// toggle clingy mode on or off.
		/// </summary>
		public void ClingToggle() {
			if(this.Mode == PonyMode.Clingy) {
				this.ClingStop();
			} else {
				this.ClingStart();
			}
		}

		/// <summary>
		/// put the pony in start clingy mode. when she is in this mood then
		/// she will continously follow the mouse cursor wherever it goes.
		/// </summary>
		private void ClingStart() {
			Trace.WriteLine(String.Format("// pony {0} get an obsessive look in her eye...", this.Name));

			this.StopAction();
			this.PauseChoiceEngine();

			this.Mode = PonyMode.Clingy;
			this.ClingTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle, this.Window.Dispatcher);
			this.ClingTimer.Interval = TimeSpan.FromMilliseconds(100);
			this.ClingTimer.Tick += new EventHandler(this.ClingTick);
			this.ClingTimer.Start();
		}

		/// <summary>
		/// stop clingy mode.
		/// </summary>
		private void ClingStop() {
			Trace.WriteLine(String.Format("// pony {0} decides she doesn't need you.",this.Name));

			this.Mode = PonyMode.Free;
			this.ClingTimer.Stop();
			this.ClingTimer = null;

			this.ResumeChoiceEngine();
		}

		/// <summary>
		/// this does the work for determining which way she should trot off
		/// to try and catch the cusor, if she should at all. this is based
		/// on distance to the cursor.
		/// 
		/// TODO: find a way that does not require Forms. the WPF version of
		/// this returns bunk values if the cursor is not currently over the
		/// window. :(
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
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

		}

		///////////////////////////////////////////////////////////////////////
		// PonyAction.Stand ///////////////////////////////////////////////////

		/// <summary>
		/// stand there and look pretty.
		/// </summary>
		private void Stand() {

		}

		///////////////////////////////////////////////////////////////////////
		// PonyAction.Trot ////////////////////////////////////////////////////

		/// <summary>
		/// put the pony in trotting mode. she will traverse the screen in
		/// whichever direction she is currently facing.
		/// </summary>
		private void Trot() {
			// inialize the timer which will run the trot animation of the window movement.
			this.WindowTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle, this.Window.Dispatcher);
			this.WindowTimer.Interval = TimeSpan.FromMilliseconds(20);
			this.WindowTimer.Tick += new EventHandler(this.TrotTick);
			this.WindowTimer.Start();
		}

		/// <summary>
		/// this does the work of making the pony traverse across the screen.
		/// it also detects if she has bumped into a wall (edge of the screen)
		/// and should change directions.
		/// 
		/// if the pony has been put into Still mode then she will switch to
		/// the standing action when hitting the wall.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void TrotTick(Object sender, EventArgs e) {
			PonyDirection direction = Direction;

			// figure out if she has wallfaced and needs to change directions.
			if(this.Direction == PonyDirection.Right) {
				if(((this.Window.Left + this.Window.Width) + 3) >= SystemParameters.PrimaryScreenWidth) {
					direction = PonyDirection.Left;
				}
			} else if(this.Direction == PonyDirection.Left) {
				if((this.Window.Left - 3) <= 0) {
					direction = PonyDirection.Right;
				}
			}

			// update the window position.
			this.Window.Left += (3 * (int)direction);

			if(direction != this.Direction) {
				Trace.WriteLine(String.Format(
					">> {0} has hit the {1} wall",
					this.Name,
					this.Direction.ToString()
				));

				// if she was trotting even though she had been told to stand
				// still, now that she has hit the wall she will turn around,
				// stand there, and look pretty.
				if(this.Mode == PonyMode.Still) {
					this.TellWhatDo(PonyAction.Stand, direction, false);
				}

				// else about face and keep going like a boss.
				else {
					this.TellWhatDo(PonyAction.Trot, direction);
				}
			}

		}

		///////////////////////////////////////////////////////////////////////
		// image management methods ///////////////////////////////////////////

		/// <summary>
		/// bring all the gifs for this pony into ram so the disk doesn't
		/// get warm like it did last night when i left the mane six runnning.
		/// </summary>
		private void LoadAllImages() {
			this.AvailableActions.ForEach(delegate(PonyAction action){
				this.Image.Add(new PonyImage(this.Name, action, PonyDirection.Left));
				this.Image.Add(new PonyImage(this.Name, action, PonyDirection.Right));
			});
		}

		/// <summary>
		/// load the graphic for the action she is currently doing.
		/// </summary>
		private void LoadImage() {
			this.LoadImage(this.Action, this.Direction);
		}

		/// <summary>
		/// find and apply a specified image from the pony's cache of them.
		/// </summary>
		/// <param name="action"></param>
		/// <param name="direction"></param>
		private void LoadImage(PonyAction action, PonyDirection direction) {
			PonyImage image;

			// find the requested image from the cache.
			image = this.Image.Find(delegate(PonyImage img){
				if(img.Action == action && img.Direction == direction) return true;
				else return false;
			});

			if(image == null) {
				Trace.WriteLine(String.Format(
					"!! no image for {0} {1} {2}",
					this.Name,
					action.ToString(),
					direction.ToString()
				));

				return;
			};

			// and apply it to the pony window.
			image.ApplyToPonyWindow(this.Window);

		}

		/////////////////////////////////////////////////////////////////////////////
		// utility methods //////////////////////////////////////////////////////////

		/// <summary>
		/// is the action she is currently doing considered an active action?
		/// </summary>
		/// <returns></returns>
		public bool IsActionActive() {
			return Pony.IsActionActive(this.Action);
		}

		/// <summary>
		/// is the specified action considered an active action?
		/// </summary>
		/// <param name="action"></param>
		/// <returns></returns>
		public static bool IsActionActive(PonyAction action) {
            return action.IsActive();
		}

	}

}
