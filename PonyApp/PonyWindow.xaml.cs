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
using System.Windows.Shapes;
using WpfAnimatedGif;
using System.Windows.Media.Animation;

namespace PonyApp {

	public partial class PonyWindow:Window {

		public Pony Pony;

		public PonyWindow(Pony Pony) {
			InitializeComponent();

			// set up the window.
			this.Left = 0;
			this.Top = 0;
			this.Width = 1;
			this.Height = 1;
			this.Title = "Pony";
			this.Topmost = true;
			this.Hide();

			// reference to the pony.
			this.Pony = Pony;

			// notice when the animation finishes.
			ImageBehavior.AddAnimationCompletedHandler(this.Image,this.OnAnimationFinish);

			// set a mouse cursor for petting lol.
			this.Cursor = Cursors.Hand;

			// enable moving the window by dragging anywhere.
			// this.MouseLeftButtonDown += new MouseButtonEventHandler(OnWindowDragAction);
		}

		public void PlaceRandomlyX() {
			this.Left = this.Pony.RNG.Next(1,((int)SystemParameters.PrimaryScreenWidth - (int)this.Width));
		}

		public void AnimateOnce() {
			ImageBehavior.SetRepeatBehavior(this.Image, new RepeatBehavior(1));
		}

		public void AnimateForever() {
			ImageBehavior.SetRepeatBehavior(this.Image, RepeatBehavior.Forever);
		}

		private void OnAnimationFinish(Object sender, RoutedEventArgs e) {
			
			switch(this.Pony.Action) {
				case PonyAction.Teleport:
					this.Pony.TeleportStage();
					break;
				case PonyAction.Teleport2: {
					this.Pony.TeleportFinish();
					break;
				}

				default:
					Trace.WriteLine("== generic animation finished");

					// this is the wrong fix but the proper use of Forever is
					// eluding me as the animation class is weird, and the
					// wpfag library adds new complexity...
					PonyImage.LoopCount(this.Pony.Window,99);
					break;
			}

		}

		private void OnWindowLoaded(object sender, RoutedEventArgs e) {

			// update the menu system with the ponies.
			PonyConfig.List.ForEach(delegate(PonyConfig pony) {
				MenuItem item;

				// add pony menu item
				item = new MenuItem();
				item.Header = pony.Name;
				item.Click += delegate(object del_s, RoutedEventArgs del_e) {
					this.OnAddPony(pony.Name);
				};
				this.AddPonyMenu.Items.Add(item);
				
				// change pony menu item
				item = new MenuItem();
				item.Header = pony.Name;
				item.Click += delegate(object del_s, RoutedEventArgs del_e) {
					this.OnChangePony(pony.Name);
				};
				this.ChangePonyMenu.Items.Add(item);

			});

		}

		private void OnWindowClosed(object sender, EventArgs e) {
			Main.StopPony(this.Pony);
		}

		private void OnDoubleClick(object sender, MouseButtonEventArgs e) {
			Trace.WriteLine("~~ window was double clicked");
			this.Pony.ClingToggle();
		}

		private void OnMouseOver(object sender, MouseEventArgs e) {
			if(this.Pony.Mode == PonyMode.Still) return;

			this.Pony.TellWhatDo(
				PonyAction.Stand,
				this.Pony.Direction,
				true
			);
		}

		private void OnMouseOut(object sender, MouseEventArgs e) {
			if(this.Pony.Mode == PonyMode.Still) return;

			this.Pony.TellWhatDo(
				PonyAction.Stand,
				this.Pony.Direction,
				false
			);
		}

		private void OnContextMenuOpen(object sender, RoutedEventArgs e) {

		}

		private void OnContextMenuClosed(object sender, RoutedEventArgs e) {

		}

		private void TellHoldToRight(object sender, RoutedEventArgs e) {
			Trace.WriteLine("## telling pony to hold short to the right");
			this.Pony.Mode = PonyMode.Still;
			this.Pony.TellWhatDo(PonyAction.Trot, PonyDirection.Right,true);
		}

		private void TellHoldToLeft(object sender, RoutedEventArgs e) {
			Trace.WriteLine("## telling pony to hold short to the left");
			this.Pony.Mode = PonyMode.Still;
			this.Pony.TellWhatDo(PonyAction.Trot, PonyDirection.Left,true);
		}

		private void TellHasFreedom(object sender, RoutedEventArgs e) {
			Trace.WriteLine("## telling pony she is free");
			this.Pony.Mode = PonyMode.Free;
		}

		private void OnTopmostPony(object sender, RoutedEventArgs e) {
			this.Pony.Window.Topmost = !this.Pony.Window.Topmost;
			((MenuItem)sender).IsChecked = this.Pony.Window.Topmost;
		}

		private void OnClosePony(object sender, RoutedEventArgs e) {
			this.OnClosePony();
		}

		private void OnClosePony() {
			this.Pony.Window.Close();
		}

		private void OnCloseAllPony(object sender, RoutedEventArgs e) {

			// I couldn't .ForEach the list because it was getting modified
			// by StopPony which was messing this up, lol.

			while(Main.PonyList.Count > 0) {
				Main.PonyList[0].Window.OnClosePony();
			}

		}

		private void OnAddPony(string name) {
			Main.StartPony(name);
		}

		private void OnChangePony(string name) {
			Main.StartPony(name);
			this.OnClosePony();
		}

	}

}
