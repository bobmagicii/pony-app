using System;
using System.Collections;
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

		private ArrayList PonyList;

		public MainWindow() {
			InitializeComponent();

			// go pony go.
			this.PonyList = new ArrayList();
			this.StartPony("Rarity");
		}

		public void StartPony(string name) {
			Pony pone;

			pone = new Pony(name);
			pone.TellWhatDo(Pony.TROT,Pony.RIGHT);

			this.PonyList.Add(pone);
		}

	}

}
