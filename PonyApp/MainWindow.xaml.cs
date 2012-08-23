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

	public partial class MainWindow:Window {

		private Pony Pony;

		public MainWindow() {
			InitializeComponent();

			// enable moving the window by dragging anywhere.
			// this.MouseLeftButtonDown += new MouseButtonEventHandler(OnWindowDragAction);

			// set the window position.
			this.Left = 0;
			this.Top = 0;
			this.Width = 1;
			this.Height = 1;
			this.Topmost = true;
			this.Title = "Pony";

			// go pony go
			this.Pony = new Pony("Rarity");
			this.Pony.SetWindow(this);
			this.Pony.TellWhatDo(Pony.TROT,Pony.RIGHT);

		}

		/* handle dragging the window since we cannot do that with the title bar
		 * removed from the form. */
		private void OnWindowDragAction(Object sender,MouseButtonEventArgs e) {
			DragMove();
		}

	}

}
