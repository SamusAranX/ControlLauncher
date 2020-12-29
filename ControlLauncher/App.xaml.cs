using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using ControlLauncher.RMDP;

namespace ControlLauncher {
	public partial class App {

		public FontFamily AkzidenzGrotesk { get; private set; }

		private void LoadFonts() {
			var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			var fontsMissing = false;
			foreach (var fontPath in RMDPExtractor.AkzidenzGroteskPaths) {
				var localFontPath = Path.Combine(exePath, Path.GetFileName(fontPath));
				if (!File.Exists(localFontPath)) {
					fontsMissing = true;
					break;
				}
			}

			if (fontsMissing) {
				Debug.WriteLine("fonts are missing, will extract them");
				try {
					RMDPExtractor.ExtractGameFiles(exePath, "ep100-000-generic", RMDPExtractor.AkzidenzGroteskPaths);
					Debug.WriteLine("font files extracted");
				} catch (Exception exception) {
					Debug.WriteLine(exception);
					// do nothing, FontFamily will be set to Inter in the XAML
				}
			}

			if (!fontsMissing) {
				Debug.WriteLine("font files found");
				var fontFamilyPath = Path.Combine(exePath, "#Akzidenz-Grotesk Pro");
				this.AkzidenzGrotesk = new FontFamily(fontFamilyPath);
			}
		}

		private void App_OnStartup(object sender, StartupEventArgs e) {
			this.LoadFonts();
		}
	}
}