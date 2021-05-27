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

		private static readonly string[] AkzidenzGroteskPaths = {
			"data/uiresources/p7/fonts/AkzidGrtskProBol.otf",
			"data/uiresources/p7/fonts/AkzidGrtskProReg.otf",
		};

		private bool CheckFontExistence(string path) {
			foreach (var fontPath in AkzidenzGroteskPaths) {
				var localFontPath = Path.Combine(path, Path.GetFileName(fontPath));
				if (!File.Exists(localFontPath))
					return false;
			}

			return true;
		}

		private void LoadFonts() {
			var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			if (!CheckFontExistence(exePath)) {
				Debug.WriteLine("fonts are missing, will extract them");
				try {
					if (!RMDPExtractor.ExtractGameFiles(exePath, "ep100-000-generic", AkzidenzGroteskPaths))
						return;

					Debug.WriteLine("font files extracted");
				} catch (Exception exception) {
					Debug.WriteLine(exception);
					// do nothing, FontFamily will be set to Inter in the XAML
				}
			}

			// recheck font existence and load them
			if (CheckFontExistence(exePath)) {
				Debug.WriteLine("font files found");
				var fontFamilyPath = Path.Combine(exePath, "#Akzidenz-Grotesk Pro");
				this.AkzidenzGrotesk = new FontFamily(fontFamilyPath);
			}
		}

		private void App_OnStartup(object sender, StartupEventArgs e) {
			if (!Helpers.IsLauncherInGameDir())
				MessageBox.Show("Please put this launcher into Control's game directory.", "Installation failed", MessageBoxButton.OK, MessageBoxImage.Warning);

			this.LoadFonts();
		}
	}
}