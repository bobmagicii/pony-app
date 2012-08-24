using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
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

		///////////////////////////////////////////////////////////////////////
		// image selecting ////////////////////////////////////////////////////

		public static Uri SelectImagePath(string name, int action, int direction) {
			
			string ImgDirection;
			switch(direction) {
				case Pony.RIGHT: ImgDirection = "Right"; break;
				case Pony.LEFT:  ImgDirection = "Left"; break;
				default:         ImgDirection = "Left"; break;
			}

			string ImgAction;
			switch(action) {
				case Pony.TROT:  ImgAction = "Trot"; break;
				case Pony.STAND: ImgAction = "Stand"; break;
				default:         ImgAction = "Stand"; break;
			}

			return new Uri(
				(AppDomain.CurrentDomain.BaseDirectory + "Resources/" + name + "/" + ImgAction + ImgDirection + ".gif"),
				UriKind.Absolute
			);
		}

		///////////////////////////////////////////////////////////////////////
		// instance work //////////////////////////////////////////////////////

		public PonyImage(string name, int action, int direction) {
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
			Trace.WriteLine("$$ uri: " + uri.ToString());
			this.img = new BitmapImage(this.uri);
		}

		public void Free() {
			this.uri = null;
			this.img = null;
		}

		public void ApplyToPonyWindow(PonyWindow win) {
			// resize the window to fit this image.
			win.Width = this.img.Width;
			win.Height = this.img.Height;

			// set the image element to the animated gif.
			ImageBehavior.SetAnimatedSource(win.Image,this.img);
		}

	}

}
