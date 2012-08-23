using System;
using System.Collections.Generic;
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
			this.Pony.ResumeAction();
		}

	}

}
