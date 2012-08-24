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

namespace PonyApp {

	public partial class PonyWindow:Window {

		private Pony Pony;

		public PonyWindow(Pony Pony) {
			InitializeComponent();

			// set up the window.
			this.Left = 0;
			this.Top = 0;
			this.Width = 1;
			this.Height = 1;
			this.Title = "Pony";
			this.Topmost = true;
			this.Show();

			// reference to the pony.
			this.Pony = Pony;


			this.Cursor = Cursors.Hand;

			// enable moving the window by dragging anywhere.
			// this.MouseLeftButtonDown += new MouseButtonEventHandler(OnWindowDragAction);
		}

		/* handle dragging the window since we cannot do that with the title bar
		 * removed from the form. */
		private void OnWindowDragAction(Object sender, MouseButtonEventArgs e) {
			DragMove();
		}

		private void OnDoubleClick(object sender, MouseButtonEventArgs e) {
			Trace.WriteLine("~~ window was double clicked");
			this.Pony.ClingToggle();
		}

		private void OnMouseOver(object sender, MouseEventArgs e) {
			this.Pony.PauseChoiceEngine();
		}

		private void OnMouseOut(object sender, MouseEventArgs e) {
			this.Pony.ResumeChoiceEngine();
		}

		private void OnContextMenuOpen(object sender, RoutedEventArgs e) {
			
		}

		private void OnContextMenuClosed(object sender, RoutedEventArgs e) {

		}

		private void TellHoldToRight(object sender, RoutedEventArgs e) {
			Trace.WriteLine("## telling pony to hold short to the right");
			this.Pony.PauseChoiceEngine();
			this.Pony.SetMode(Pony.BE_STILL);
			this.Pony.TellWhatDo(Pony.TROT,Pony.RIGHT);
		}

		private void TellHoldToLeft(object sender, RoutedEventArgs e) {
			Trace.WriteLine("## telling pony to hold short to the left");
			this.Pony.PauseChoiceEngine();
			this.Pony.SetMode(Pony.BE_STILL);
			this.Pony.TellWhatDo(Pony.TROT, Pony.LEFT);
		}

		private void TellHasFreedom(object sender, RoutedEventArgs e) {
			Trace.WriteLine("## telling pony she is free");
			this.Pony.SetMode(Pony.BE_FREE);
		}

		private void OnPonyClosed(object sender, EventArgs e) {
			Main.StopPony(this.Pony);
		}

		private void OnTopmostPony(object sender, RoutedEventArgs e) {
			this.Pony.Window.Topmost = !this.Pony.Window.Topmost;
			((MenuItem)sender).IsChecked = this.Pony.Window.Topmost;
		}

		private void OnClosePony(object sender, RoutedEventArgs e) {
			this.Pony.Window.Close();
		}

	}

}
