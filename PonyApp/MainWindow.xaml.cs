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

	class Main {

		// a list of all the ponies running around.
		public static ArrayList PonyList = new ArrayList();

		// start and track a new pony.
		public static void StartPony(string name) {
			Pony pone = new Pony(name);
			Main.PonyList.Add(pone);
		}

		// stop and release an old pony.
		public static void StopPony(Pony pone) {
			Main.PonyList.Remove(pone);
			pone.Window = null;
			pone = null;

			GC.Collect();

			if(Main.PonyList.Count == 0) {
				Application.Current.Shutdown();
			}
		}
	}

	public partial class MainWindow:Window {

		public MainWindow() {
			InitializeComponent();

			// go pony go.
			Main.StartPony("Rarity");
		}

	}

}
