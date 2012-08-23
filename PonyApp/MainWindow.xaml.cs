using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

	public partial class MainWindow:Window {


		// this timer is for moving the window around. unfortunately if the pc is too busy
		// or i set it too fast, it slow down the gif rendering quite a bit as it shares
		// the ui thread. problem is that system.timers.timer didn't have permissions to
		// update the object owned by another thread.
		private DispatcherTimer WindowTimer;

		// this timer is for when the pony decides it should do something else. time till
		// bordem basically.
		private DispatcherTimer ChoiceTimer;

		// the current action the pony is taking and the current direction. these need to
		// get moved to the pony class when i get to making the pony class work.
		private int CurrentAction;
		private int CurrentDirection;
		private BitmapImage CurrentImage;

		// just some things i stored because i can.
		private double ScreenWidth;
		private double ScreenHeight;

		public MainWindow() {
			InitializeComponent();

			// enable moving the window by dragging anywhere.
			// this.MouseLeftButtonDown += new MouseButtonEventHandler(OnWindowDragAction);

			// prepare some values.
			this.ScreenWidth = SystemParameters.VirtualScreenWidth;
			this.ScreenHeight = SystemParameters.VirtualScreenHeight;
			this.WindowTimer = null;
			this.ChoiceTimer = null;
			this.CurrentAction = Pony.STAND;
			this.CurrentDirection = Pony.RIGHT;

			// set the window position.
			this.Left = 0;
			this.Top = 0;
			this.Width = 1;
			this.Height = 1;
			this.Topmost = true;
			this.Title = "Pony";

			// go pony go
			this.ChooseWhatDo(Pony.TROT,Pony.RIGHT);
			this.Top = this.ScreenHeight - (32 + this.Height);

			// setup a timer for choosing what to do.
			this.ChoiceTimer = new DispatcherTimer(DispatcherPriority.Normal,this.Dispatcher);
			this.ChoiceTimer.Interval = TimeSpan.FromSeconds(1);
			this.ChoiceTimer.Tick += new EventHandler(this.ChooseWhatDo);
			this.ChoiceTimer.Start();

		}



		private void CurrentActionStop() {

			if(this.WindowTimer != null) {
				this.WindowTimer.Stop();
				this.WindowTimer = null;
				// this is me assuming there is a garbage collector in this
				// language and that it is doing its goddamn job :)
			}

		}

		/* void MainWindow.ChooseWhatToDo(int action, int direction);
		 * this version of the method is the authority. what it is told is what
		 * the pony is made to do */

		private void ChooseWhatDo(int action,int direction) {
			this.CurrentActionStop();

			this.CurrentAction = action;
			this.CurrentDirection = direction;

			switch(action) {
				case Pony.TROT:
					this.Trot(direction);
					break;

				case Pony.STAND:
					this.Stand(direction);
					break;
			}

			Trace.WriteLine("choice: " + this.CurrentAction.ToString() + ":" + this.CurrentDirection.ToString());
		}

		/* void MainWindow.ChooseWhatToDo(Object sender EventArgs event);
		 * this version of the method is for calling by the ChoiceTimer. It resets
		 * the choice timer and then allows the pony to decide what to do. */

		private void ChooseWhatDo(Object s,EventArgs e) {
			int time = 0;
			Random rng = new Random();

			// stop choice timer.
			this.ChoiceTimer.Stop();

			// choose what to do.
			this.ChooseWhatDo();

			// restart the choice timer. ponies like to be lazy so
			// choose sooner if we are doing something active.
			if(this.CurrentAction == Pony.TROT) time = rng.Next(3,5);
			else time = rng.Next(12,20);

			this.ChoiceTimer.Interval = TimeSpan.FromSeconds(time);
			this.ChoiceTimer.Start();
		}

		/* void MainWindow.ChooseWhatToDo(void);
		 * this version of the method is the default to allow the pony to decide
		 * what it wants to do next. */

		private void ChooseWhatDo() {
			int[] choice = this.ChoiceByPersonality();
			this.ChooseWhatDo(choice[0],choice[1]);
		}

		/* int-array MainWindow.ChoiceByPersonality(void);
		 * choose what to do based on the pony personality. at the moment i only
		 * have one personality, the personality i want. it will choose the new
		 * action and direction. Returns an array 0 being the action she selected
		 * and 1 being the direction chosen. */

		private int[] ChoiceByPersonality() {
			Random rng = new Random();
			bool undecided = false;
			int action = 0;
			int direction = rng.Next(1,3);

			do {

				action = rng.Next(1,3);
				direction = rng.Next(1,3);
				undecided = false;

				// pony generally like be lazy. if she is doing somethng lazy there is a
				// greater chance she will not want to do something active.
				if(Pony.IsActionActive(action) && !Pony.IsActionActive(this.CurrentAction)) {
					undecided = true;
				}

				// if pony is doing something active and will continue to do so then there
				// is a higher chance she will not change directions.
				if(Pony.IsActionActive(action) && Pony.IsActionActive(this.CurrentAction)) {
					if(rng.Next(1,101) <= 70) {
						direction = this.CurrentDirection;
					}
				}

				// if pony is doing something active and she suddenly stops then this too
				// will have a greater chance of not changing directions.
				if(Pony.IsActionActive(action) && !Pony.IsActionActive(this.CurrentAction)) {
					if(rng.Next(1,101) <= 70) {
						direction = this.CurrentDirection;
					}
				}

			} while(undecided);

			int[] choice = new int[2];
			choice[0] = action;
			choice[1] = direction;
			return choice;
		}

		/*
		 * STANDING
		 * all you have to do is look pretty.
		 */

		private void Stand(int dir) {
			this.CurrentDirection = dir;
			this.LoadImage();
		}

		/*
		 * TROTTING
		 * walking back, and forth, and back, and forth.
		 */

		private void Trot(int dir) {
			this.CurrentDirection = dir;
			this.LoadImage();

			this.WindowTimer = new DispatcherTimer(DispatcherPriority.Normal,this.Dispatcher);
			this.WindowTimer.Interval = TimeSpan.FromMilliseconds(25);
			this.WindowTimer.Tick += new EventHandler(this.TrotMove);
			this.WindowTimer.Start();
		}

		private void TrotMove(Object sender,EventArgs e) {

			// if trotting to the right detect hitting the right edge.
			if(this.CurrentDirection == 1) {
				if((this.Left + this.Width) + 3 >= this.ScreenWidth) {
					this.CurrentActionStop();
					Trace.WriteLine("hit the right wall");
					this.Trot(2);
				} else {
					this.Left += 3;
				}
			}

			// if trotting to the left detect hitting the left edge.
			if(this.CurrentDirection == 2) {
				if(this.Left - 3 <= 0) {
					this.CurrentActionStop();
					Trace.WriteLine("hit the left wall");
					this.Trot(1);
				} else {
					this.Left -= 3;
				}
			}

		}

		/* handle dragging the window since we cannot do that with the title bar
		 * removed from the form. */
		private void OnWindowDragAction(Object sender,MouseButtonEventArgs e) {
			DragMove();
		}

		private void LoadImage() {
			int act = this.CurrentAction;
			int dir = this.CurrentDirection;
			String src = null;

			switch(act) {
				case 1: {
						if(dir == 1) src = "pack://application:,,,/Resources/Rarity/TrotRight.gif";
						if(dir == 2) src = "pack://application:,,,/Resources/Rarity/TrotLeft.gif";
						break;
					}
				case 2: {
						if(dir == 1) src = "pack://application:,,,/Resources/Rarity/StandRight.gif";
						if(dir == 2) src = "pack://application:,,,/Resources/Rarity/StandLeft.gif";
						break;
					}
			}

			BitmapImage pony = this.CurrentImage = new BitmapImage(new Uri(src));
			this.Width = pony.Width;
			this.Height = pony.Height;
			ImageBehavior.SetAnimatedSource(this.PonyImage,pony);

		}

	}

}
