using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using ControlLauncher.RMDP;

namespace ControlLauncher;

public partial class App
{
	public FontFamily? AkzidenzGrotesk { get; private set; }

	private static readonly string[] AKZIDENZ_GROTESK_PATHS =
	{
		"data/uiresources/p7/fonts/AkzidGrtskProBol.otf",
		"data/uiresources/p7/fonts/AkzidGrtskProReg.otf",
	};

	private static bool CheckFontExistence(string path)
	{
		foreach (var fontPath in AKZIDENZ_GROTESK_PATHS)
		{
			var localFontPath = Path.Combine(path, Path.GetFileName(fontPath));
			if (!File.Exists(localFontPath))
				return false;
		}

		return true;
	}

	private void LoadFonts()
	{
		var exePath = AppContext.BaseDirectory;

		if (!CheckFontExistence(exePath))
		{
			Debug.WriteLine("fonts are missing, will extract them");
			try
			{
				if (!RMDPExtractor.ExtractGameFiles(exePath, "ep100-000-generic", AKZIDENZ_GROTESK_PATHS))
					return;

				Debug.WriteLine("font files extracted");
			}
			catch (Exception exception)
			{
				Debug.WriteLine(exception); // do nothing, FontFamily will be set to Inter in the XAML
			}
		}

		// recheck font existence and load them
		if (CheckFontExistence(exePath))
		{
			Debug.WriteLine("font files found");
			var fontFamilyPath = Path.Combine(exePath, "#Akzidenz-Grotesk Pro");
			this.AkzidenzGrotesk = new FontFamily(fontFamilyPath);
		}
	}

	private void App_OnStartup(object sender, StartupEventArgs e)
	{
		if (!Helpers.IsLauncherInGameDir())
			MessageBox.Show("Please put this launcher into Control's game directory.", "Installation failed", MessageBoxButton.OK, MessageBoxImage.Warning);
		else
		{
			var args = Helpers.GetCommandLineArgs();
			if (args.Contains("-dx11", StringComparer.OrdinalIgnoreCase))
			{
				Debug.WriteLine("Launching DX11 because -dx11");
				Launcher.LaunchDX11(args);
				this.Shutdown();
				return;
			}

			if (args.Contains("-dx12", StringComparer.OrdinalIgnoreCase))
			{
				Debug.WriteLine("Launching DX12 because -dx12");
				Launcher.LaunchDX12(args);
				this.Shutdown();
				return;
			}

			if (!Helpers.InstallRedistributableIfNeeded())
			{
				MessageBox.Show("Microsoft Visual C++ Redistributable installation failed.", "Installation failed", MessageBoxButton.OK, MessageBoxImage.Error);
				this.Shutdown(1);
			}
			else if (Helpers.IsWin7OrWin8())
			{
				Debug.WriteLine("Launching DX11 because Win 7/8 detected");
				Launcher.LaunchDX11(args);
				this.Shutdown();
			}
			else if (!Helpers.CheckForRaytracing())
			{
				Debug.WriteLine("Launching DX11 because no RT");
				Launcher.LaunchDX11(args);
				this.Shutdown();
			}
		}

		this.LoadFonts();
	}
}
