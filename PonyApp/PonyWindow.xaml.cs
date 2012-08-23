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

			// enable moving the window by dragging anywhere.
			// this.MouseLeftButtonDown += new MouseButtonEventHandler(OnWindowDragAction);
		}

		/* handle dragging the window since we cannot do that with the title bar
		 * removed from the form. */
		private void OnWindowDragAction(Object sender, MouseButtonEventArgs e) {
			DragMove();
		}

		private void OnContextMenuOpen(object sender, RoutedEventArgs e) {
			this.Pony.PauseAction();
		}

		private void OnContextMenuClosed(object sender, RoutedEventArgs e) {
			// i think i can count on this always firing after the onclick's
			// of menu items?

			if(this.Pony.GetMode() != Pony.BE_STILL)
			this.Pony.ResumeAction();
		}

		private void TellHoldToRight(object sender, RoutedEventArgs e) {
			Trace.WriteLine("## telling pony to hold short to the right");
			this.Pony.SetMode(Pony.BE_STILL);
			this.Pony.TellWhatDo(Pony.TROT,Pony.RIGHT);
		}

		private void TellHoldToLeft(object sender, RoutedEventArgs e) {
			Trace.WriteLine("## telling pony to hold short to the left");
			this.Pony.SetMode(Pony.BE_STILL);
			this.Pony.TellWhatDo(Pony.TROT, Pony.LEFT);
		}

		private void TellHasFreedom(object sender, RoutedEventArgs e) {
			Trace.WriteLine("## telling pony she is free");
			this.Pony.SetMode(Pony.BE_FREE);
		}

	}

}
