using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

namespace PonyApp {
	public class PonyIcon {

		public NotifyIcon Icon;

		public PonyIcon(string name) {
			string path;

			// check that the icon we want exists.
			path = PonyImage.SelectImagePath(name,"MarkSmall.png").LocalPath;
			if(!File.Exists(path)) return;

			// create the tray icon.
			this.Icon = new NotifyIcon();

			// load the image and convert it to an icon resource.
			this.Icon.Icon = System.Drawing.Icon.FromHandle((new Bitmap(path)).GetHicon());

			this.Show();
			return;
		}

		public void Free() {
			if(this.Icon != null) {
				this.Hide();
				this.Icon.Dispose();
				this.Icon = null;
			}
		}

		public void Hide() {
			if(this.Icon == null) return;
			this.Icon.Visible = false;
		}

		public void Show() {
			if(this.Icon == null) return;
			this.Icon.Visible = true;
		}


	}

}
