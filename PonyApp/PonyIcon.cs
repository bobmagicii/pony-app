using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

namespace PonyApp {
	public class PonyIcon {

		public Pony Pony;
		public NotifyIcon Icon;

		public PonyIcon(Pony pone) {
			string path;

			this.Pony = pone;

			// check that the icon we want exists.
			path = PonyImage.SelectImagePath(pone.Name,"MarkSmall.png").LocalPath;
			if(!File.Exists(path)) return;

			// create the tray icon.
			this.Icon = new NotifyIcon();

			// load the image and convert it to an icon resource.
			this.Icon.Icon = System.Drawing.Icon.FromHandle((new Bitmap(path)).GetHicon());

			// connect some signals.
			this.Icon.MouseClick += this.OnMouseClick;

			this.Show();
			return;
		}

		public void OnMouseClick(object sender, EventArgs e) {
			this.Pony.Window.MainMenu.IsOpen = true;
			this.Pony.Window.MainMenu.Focus();
		}

		public void Free() {
			if(this.Icon != null) {
				this.Hide();
				this.Icon.Dispose();
				this.Icon = null;
			}

			this.Pony = null;
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
