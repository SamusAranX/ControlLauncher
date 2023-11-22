using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace ControlLauncher;

internal sealed class Launcher
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
		if (Helpers.DEBUG)
		{
			// makes for easier debugging
			var argsString = string.Join(" ", args).Trim();
			var argsMessage = argsString == "" ? "no args" : $"args \"{argsString}\"";
			MessageBox.Show($"Starting {relativeExePath} with {argsMessage}");
			return true;
		}

		try
		{
			var process = new Process
			{
				StartInfo =
				{
					FileName = relativeExePath,
					Arguments = string.Join(" ", args),
					WorkingDirectory = AppContext.BaseDirectory,
					UseShellExecute = false,
					RedirectStandardOutput = false,
					RedirectStandardError = false,
					RedirectStandardInput = false,
				},
			};
			process.Start();
			return true;
		}
		catch (Win32Exception ex)
		{
			// error codes: https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-erref/18d8fbe8-a967-4f1c-ae50-99ca8e491d2d
			switch (ex.NativeErrorCode)
			{
				case 2: // ERROR_FILE_NOT_FOUND
					MessageBox.Show("Error launching the game: The launcher is not in Control's game directory", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					break;
				default:
					MessageBox.Show($"Error launching the game: Unknown error (Code 0x{ex.NativeErrorCode:08X})", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					break;
			}
		}
		catch (Exception ex)
		{
			Trace.WriteLine(ex.GetType());
			MessageBox.Show("Error launching the game: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		}

		return false;
	}
}
