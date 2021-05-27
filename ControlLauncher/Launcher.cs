﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace ControlLauncher {
	internal class Launcher {
		public static bool LaunchDX11(string[] args) {
			return Launch("Control_DX11.exe", args);
		}

		public static bool LaunchDX12(string[] args) {
			return Launch("Control_DX12.exe", args);
		}

		private static bool Launch(string relativeExePath, string[] args) {
#if DEBUG
			// makes for easier debugging
			var argsString = string.Join(" ", args).Trim();
			var argsMessage = argsString == "" ? "no args" : $"args \"{argsString}\"";
			MessageBox.Show($"Starting {relativeExePath} with {argsMessage}");
			return true;
#endif

			try {
				var executablePath = Assembly.GetExecutingAssembly().Location;
				var directoryName = Path.GetDirectoryName(executablePath) ?? @"\";
				var process = new Process {
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
			} catch (Win32Exception ex) {
				switch(ex.NativeErrorCode) {
					case 2:
						MessageBox.Show("Error launching the game: The launcher is not in Control's game directory", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
						break;
					default:
						MessageBox.Show($"Error launching the game: Unknown error (Code {ex.NativeErrorCode})", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
						break;
				}
			} catch (Exception ex) {
				Trace.WriteLine(ex.GetType());
				MessageBox.Show("Error launching the game: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}

			return false;
		}
	}
}