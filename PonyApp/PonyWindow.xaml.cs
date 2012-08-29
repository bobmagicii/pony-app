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
using System.Drawing;

namespace PonyApp {

	public partial class PonyWindow:Window {

		public Pony Pony;
		public PonyIcon Tray;

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

			// tray icon.
			this.Tray = new PonyIcon(this.Pony.Name);
			this.Tray.Show();

			// set a mouse cursor for petting lol.
			this.Cursor = System.Windows.Input.Cursors.Hand;

			// enable moving the window by dragging anywhere.
			// this.MouseLeftButtonDown += new MouseButtonEventHandler(OnWindowDragAction);
		}

		/// <summary>
		/// releases the resources and references this window has.
		/// </summary>
		public void Free() {

			// dispose of tray icon.
			this.Tray.Hide();
			this.Tray.Free();
			this.Tray = null;

			// release references.
			this.Pony = null;

		}

		/// <summary>
		/// moves this window to a random location on the x axis taking into
		/// consideration the width of the window to make sure it spawns on
		/// screen.
		/// </summary>
		public void PlaceRandomlyX() {
			this.Left = this.Pony.RNG.Next(1,((int)SystemParameters.PrimaryScreenWidth - (int)this.Width));
		}

		/// <summary>
		/// set the animation to only loop once and then callback.
		/// </summary>
		public void AnimateOnce() {
			ImageBehavior.SetRepeatBehavior(this.Image, new RepeatBehavior(1));
			ImageBehavior.AddAnimationCompletedHandler(this.Image,this.OnAnimationFinish);
		}

		/// <summary>
		/// set the animation to loop forever and disable the callback.
		/// </summary>
		public void AnimateForever() {
			ImageBehavior.SetRepeatBehavior(this.Image, RepeatBehavior.Forever);
			ImageBehavior.RemoveAnimationCompletedHandler(this.Image,this.OnAnimationFinish);
		}

		/// <summary>
		/// the callback for when the animations end, useful for staging the
		/// next phase.
		/// </summary>
		private void OnAnimationFinish(Object sender, RoutedEventArgs e) {

			switch(this.Pony.Action) {
				case PonyAction.Teleport:
					this.Pony.TeleportStage();
					break;
				case PonyAction.Teleport2: {
					this.Pony.TeleportFinish();
					break;
				}
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

			// reset the mode checkboxes.
			this.StandLeft.IsChecked  =
			this.StandRight.IsChecked =
			this.BeFree.IsChecked     =
			false;

			// decide the mode box to check.
			switch(this.Pony.Mode) {
				case PonyMode.Free:
					this.BeFree.IsChecked = true;
					break;
				case PonyMode.Still:
					if(this.Left < 10) this.StandLeft.IsChecked = true;
					else this.StandRight.IsChecked = true;
					break;
			}

			// decide the on top checkbox.
			this.OnTop.IsChecked = this.Topmost;

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
