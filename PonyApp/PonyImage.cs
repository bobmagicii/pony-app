using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Timers;
using WpfAnimatedGif;

namespace PonyApp {

	public class PonyImage {

		public Uri uri;
		public BitmapImage img;

		public PonyAction Action;
		public PonyDirection Direction;

		///////////////////////////////////////////////////////////////////////
		// image selecting ////////////////////////////////////////////////////

		public static Uri SelectImagePath(string name, PonyAction action, PonyDirection direction) {
			return new Uri(
				String.Format(
					"{0}Resources\\{1}\\{2}{3}.gif",
					AppDomain.CurrentDomain.BaseDirectory,
					name,
					action.ToString(),
					direction.ToString()
				),
				UriKind.Absolute
			);
		}

		///////////////////////////////////////////////////////////////////////
		// instance work //////////////////////////////////////////////////////

		public PonyImage(string name, PonyAction action, PonyDirection direction) {
			this.Action = action;
			this.Direction = direction;
			this.Load(PonyImage.SelectImagePath(name,action,direction));
		}

		public void Load(Uri uri) {
			this.uri = uri;
			this.Load();
		}

		public void Load(string path) {
			this.uri = new Uri(path);
			this.Load();
		}

		public void Load() {
			// Trace.WriteLine("$$ uri: " + uri.ToString());

			if(!File.Exists(this.uri.LocalPath)) {
				this.img = null;
			} else {
				Trace.WriteLine(String.Format(
					"[IMG] {0}",
					this.uri.LocalPath
				));
				this.img = new BitmapImage();
				this.img.BeginInit();
				this.img.UriSource = this.uri;
				this.img.CacheOption = BitmapCacheOption.OnLoad;
				this.img.EndInit();
			}
			
		}

		public void Free() {
			this.uri = null;
			this.img = null;
		}

		public void ApplyToPonyWindow(PonyWindow win) {
			if(this.img == null) return;

			// resize the window to fit this image.
			win.Width = this.img.Width;
			win.Height = this.img.Height;

			// set the image element to the animated gif.
			ImageBehavior.SetAnimatedSource(win.Image,this.img);

		}

		public static void LoopCount(PonyWindow win, double count) {
			ImageBehavior.SetRepeatBehavior(
				win.Image,
				new System.Windows.Media.Animation.RepeatBehavior(count)	
			);
			return;
		}
		

	}
	
}
