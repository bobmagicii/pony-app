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
			Bitmap icon;
			string path;

			this.Icon = new NotifyIcon();

			path = PonyIcon.SelectIconPath(name).LocalPath;
			if(!File.Exists(path)) return;

			icon = new Bitmap(path);
			this.Icon.Icon = System.Drawing.Icon.FromHandle(icon.GetHicon());

			this.Show();
			return;
		}

		public void Hide() {
			this.Icon.Visible = false;
		}

		public void Show() {
			this.Icon.Visible = true;
		}

		public static Uri SelectIconPath(string name) {
			return new Uri(
				String.Format(
					"{0}Resources\\{1}\\MarkSmall.png",
					AppDomain.CurrentDomain.BaseDirectory,
					name
				),
				UriKind.Absolute
			);
		}

	}

}
