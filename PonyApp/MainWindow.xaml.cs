﻿using System;
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

	static class Main {

		/// <summary>
		/// a list of all ponies that are currently running around on the
		/// desktop. well, maybe not literally running. they could just be
		/// standing there too.
		/// </summary>
		public static List<Pony> PonyList = new List<Pony>();

		/// <summary>
		/// attempt to start the requested pony, checking that it is one from
		/// the configuration system.
		/// </summary>
		public static void StartPony(string name) {
			PonyConfig pconfig;

			// the pony you seek, i may have heard of her long ago...
			pconfig = PonyConfig.List.Find(delegate(PonyConfig config){
				return config.Name == name;
			});

			if(pconfig == null) {
				// ... but it was only in legend.
				Trace.WriteLine(String.Format(
					"== unable to start {0}",
					name
				));

				return;
			} else {
				// ... yeah she lives next door.
				Pony pone = new Pony(pconfig);
				Main.PonyList.Add(pone);
			}

		}

		/// <summary>
		/// stop a currently running pony.
		/// </summary>
		public static void StopPony(Pony pone) {
			Main.PonyList.Remove(pone);
			pone.Free();
			pone = null;
			

			if(Main.PonyList.Count == 0) {
				Application.Current.Shutdown();
			}
		}

		public static string ToPoniesString(this List<Pony> PonyList) {
			string ponies = String.Empty;

			for (int i = 0; i < Main.PonyList.Count; i++)
			{
				ponies += $"\"{Main.PonyList[i].Name}\" ";
			}

			return ponies;
		}
	}

	public partial class MainWindow:Window {

		public MainWindow() {
			InitializeComponent();

			string[] args = Environment.GetCommandLineArgs();
			bool cmdpony = false;

			// find anypony that is available.
			PonyConfig.FindEverypony();
			Trace.WriteLine(String.Format(
				"== Found {0} usable ponies",
				PonyConfig.List.Count
			));

			// go pony go.
		
			// check to see if any ponies were specified on the command line.
			if (args.Length > 1) {
				foreach (string arg in args) {
					if (PonyConfig.List.Exists(delegate(PonyConfig pc) {
						return (pc.Name == arg);
					})) {
						cmdpony = true;
						Main.StartPony(arg);
					}
				}
			}

			// if the command line did not successfully spawn ponies then start one now.
			if(!cmdpony) Main.StartPony("Twilight Sparkle");

		}

	}

}
