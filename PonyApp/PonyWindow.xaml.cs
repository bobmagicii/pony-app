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
using System.IO;
using SimpleMaid;

namespace PonyApp {

	public partial class PonyWindow:Window {

		public Pony Pony;
		public string Ponies;
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
			this.Tray = new PonyIcon(this.Pony);
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
			System.Windows.Controls.Image icon;
			Uri iconpath;

			// pony name menu item.
			iconpath = PonyImage.SelectImagePath(this.Pony.Name,"MarkSmall.png");
			if(File.Exists(iconpath.LocalPath)) {
				icon = new System.Windows.Controls.Image();
				icon.Source = new BitmapImage(iconpath);
				this.Menu_PonyName.Icon = icon;
			}
			this.Menu_PonyName.Header = this.Pony.Name;

			// hide/show the time of day sleep option.
			if(!this.Pony.CanDo(PonyAction.Sleep)) 
				this.SleepTOD.Visibility = System.Windows.Visibility.Collapsed;
			else
				this.SleepTOD.Visibility = System.Windows.Visibility.Visible;
			

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

			// update the menu system with actions this pony can do.
			this.Pony.AvailableActions.ForEach(delegate(PonyAction a){
				MenuItem item;

				item = new MenuItem();
				item.Header = String.Format("{0} {1}",a.ToString(),PonyDirection.Left.ToString());
				item.Click += delegate(object del_s, RoutedEventArgs del_e) {
					this.Pony.TellWhatDo(a,PonyDirection.Left,false);
				};
				this.OtherActions.Items.Add(item);

				item = new MenuItem();
				item.Header = String.Format("{0} {1}", a.ToString(), PonyDirection.Right.ToString());
				item.Click += delegate(object del_s, RoutedEventArgs del_e) {
					this.Pony.TellWhatDo(a,PonyDirection.Right,false);
				};
				this.OtherActions.Items.Add(item);
			});

		}

		private void OnWindowClosed(object sender, EventArgs e) {
			BringPoniesIntoView();
			Main.StopPony(this.Pony);
		}

		private void OnClick(object sender, MouseButtonEventArgs e) {
			if(this.Pony.Action == PonyAction.Sleep) {
				this.Pony.TellWhatDo(
					PonyAction.Stand,
					this.Pony.Direction,
					false
				);				
			}
		}

		private void OnDoubleClick(object sender, MouseButtonEventArgs e) {
			this.Pony.ClingToggle();
		}

		private void OnMouseOver(object sender, MouseEventArgs e) {
			if(this.Pony.Action == PonyAction.Sleep) return;
			if(this.Pony.Mode == PonyMode.Still) return;

			this.Pony.TellWhatDo(
				PonyAction.Stand,
				this.Pony.Direction,
				true
			);
		}

		private void OnMouseOut(object sender, MouseEventArgs e) {
			if(this.Pony.Action == PonyAction.Sleep) return;
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
			this.TopmostPony.IsChecked = this.Topmost;
			this.SleepTOD.IsChecked = this.Pony.SleepTOD;

			// decide the autorun checkbox
			Ponies = Main.PonyList.ToPoniesString();
			this.AutorunStartup.IsChecked = SimpleApp.VerifyAutorun(System.Windows.Forms.Application.ProductName, $"{System.Windows.Forms.Application.ExecutablePath} {Ponies}");


			// fade out the other ponies.
			Main.PonyList.ForEach(delegate(Pony p){
				p.Window.Opacity = 0.4;
			});
			this.Opacity = 1.0;

			
		}

		private void BringPoniesIntoView() {
			Main.PonyList.ForEach(delegate (Pony p) {
				p.Window.Opacity = 1.0;
			});
		}

		private void OnContextMenuClosed(object sender, RoutedEventArgs e) {
			BringPoniesIntoView();
		}

		private void UpdatePoniesAutorun(string ponies = null) {
			if (this.AutorunStartup.IsChecked)
			{
				Ponies = ponies ?? Main.PonyList.ToPoniesString();
				SimpleApp.SwitchAutorun(System.Windows.Forms.Application.ProductName, $"{System.Windows.Forms.Application.ExecutablePath} {Ponies}");
			}
			else
				SimpleApp.SwitchAutorun(System.Windows.Forms.Application.ProductName);
		}

		private void MorningInPonyville(object sender, RoutedEventArgs e) {
			Trace.WriteLine("## launching pony app when the system starts");
			UpdatePoniesAutorun();
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
		}

		private void OnClosePony(object sender, RoutedEventArgs e) {
			this.OnClosePony();
		}

		private void OnClosePony() {
			this.Pony.Window.Close();

			if (Main.PonyList.Count >= 1)
				UpdatePoniesAutorun();
		}

		private void OnCloseAllPony(object sender, RoutedEventArgs e) {

			string tempPonies = Ponies;

			// I couldn't .ForEach the list because it was getting modified
			// by StopPony which was messing this up, lol.

			while(Main.PonyList.Count > 0) {
				Main.PonyList[0].Window.OnClosePony();
			}

			UpdatePoniesAutorun(tempPonies); // HACK: Returns autorun to what it was before OnCloseAllPony got invoked

		}

		private void OnAddPony(string name) {
			Main.StartPony(name);
			UpdatePoniesAutorun();
		}

		private void OnChangePony(string name) {
			Main.StartPony(name);
			UpdatePoniesAutorun();
			this.OnClosePony();
		}

		private void OnSleepTOD(object sender, RoutedEventArgs e) {
			this.Pony.SleepTOD = !this.Pony.SleepTOD;
		}

	}

}
