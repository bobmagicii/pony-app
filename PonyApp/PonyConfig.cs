using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace PonyApp {

	public class PonyConfig {

		/// <summary>
		/// both the name of the pony and the directory in which it resides.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// the list of actions that this pony is able to perform.
		/// </summary>
		public List<PonyAction> Actions { get; set; } 

		/// <summary>
		/// the y-offset for the graphic. some ponies have teleport effects and
		/// whatnot so they might have more space below them than others do.
		/// </summary>
		public int YOffset { get; set; }

		/////////////////////////////////////////////////////////////////////////////
		/////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// a static list of all the ponies found and good to go.
		/// </summary>
		public static List<PonyConfig> List = new List<PonyConfig>();

		/// <summary>
		/// Scan the resource directory for all the ponies currently available
		/// in there. Each one should have its own this.pony file which is to
		/// be a JSON structure of the PonyConfig.
		/// </summary>
		public static void FindEverypony() {
			string dotpony;
			PonyConfig config;

			// go through the resource directory looking for pony folders.
			DirectoryInfo dir = new DirectoryInfo(String.Format(
				"{0}Resources\\",
				AppDomain.CurrentDomain.BaseDirectory
			));

			foreach(DirectoryInfo ponydir in dir.GetDirectories()) {

				// in the pony folders check for config files.
				dotpony = String.Format("{0}\\this.pony",ponydir.FullName);
				if(!File.Exists(dotpony)) {
					continue;
				}
		
				// load her config.
				// needs more error checking. probably.
				config = JsonConvert.DeserializeObject<PonyConfig>(File.ReadAllText(dotpony));

				if(config == null) {
					Trace.WriteLine(String.Format("[JSON] ERROR loading {0}",ponydir.Name));
					continue;
				}

				Trace.WriteLine(String.Format(
					"[JSON] SUCCESS Name: {0}, Actions Count: {1}",
					config.Name,
					config.Actions.Count
				));

				PonyConfig.List.Add(config);

			}

			return;
		}




	}
}
