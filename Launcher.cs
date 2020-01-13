using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace ControlLauncher
{
	internal class Launcher
	{
		public static bool LaunchDX11(string[] args)
		{
			return Launch("Control_DX11.exe", args);
		}

		public static bool LaunchDX12(string[] args)
		{
			return Launch("Control_DX12.exe", args);
		}

		private static bool Launch(string relativeExePath, string[] args)
		{
			try {
				var executablePath = Assembly.GetExecutingAssembly().Location;
				var directoryName = Path.GetDirectoryName(executablePath) ?? @"\";
				var process = new Process
				{
					StartInfo =
					{
						FileName = relativeExePath,
						Arguments = string.Join(" ", args),
						WorkingDirectory = directoryName,
						UseShellExecute = false,
						RedirectStandardOutput = false,
						RedirectStandardError = false,
						RedirectStandardInput = false
					}
				};
				process.Start();
				return true;
			} catch (Exception ex) {
				MessageBox.Show("Error launching the game: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}
		}
	}
}