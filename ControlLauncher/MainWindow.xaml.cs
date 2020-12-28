using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ControlLauncher.RMDP;

namespace ControlLauncher {
	public partial class MainWindow {
		public MainWindow() {
			this.InitializeComponent();
		}

		private void MainWindow_OnLoaded(object sender, RoutedEventArgs e) {
			var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			var fontsMissing = false;
			foreach (var fontPath in RMDPExtractor.FontPaths) {
				var localFontPath = Path.Combine(exePath, Path.GetFileName(fontPath));
				if (!File.Exists(localFontPath)) {
					fontsMissing = true;
					break;
				}
			}

			if (fontsMissing) {
				Debug.WriteLine("fonts are missing, will extract them");
				try {
					RMDPExtractor.ExtractGameFiles(exePath, "ep100-000-generic", RMDPExtractor.FontPaths);
					Debug.WriteLine("font files extracted");
				} catch (Exception exception) {
					Debug.WriteLine(exception);
					throw;
					// do nothing, FontFamily will be set to Inter in the XAML
				}
			}

			if (!fontsMissing) {
				Debug.WriteLine("font files found");
				var fontFamilyPath = Path.Combine(exePath, "#Akzidenz-Grotesk Pro");
				this.FontFamily = new FontFamily(fontFamilyPath);
				// set font here
			}

			var args = Environment.CommandLine.Split();
			foreach (var str in args) {
				var dx11 = str.Equals("-dx11", StringComparison.InvariantCultureIgnoreCase);
				var dx12 = str.Equals("-dx12", StringComparison.InvariantCultureIgnoreCase);
				if (dx11) {
					Launcher.LaunchDX11(args);
					this.Close();
				} else if (dx12) {
					Launcher.LaunchDX12(args);
					this.Close();
				}
			}

			if (!Helpers.CheckForCDll()) {
				MessageBox.Show("Microsoft Visual C++ Redistributable installation failed.", "Installation failed", MessageBoxButton.OK, MessageBoxImage.Error);
				this.Close();
			} else if (Helpers.IsWin7OrWin8()) {
				Launcher.LaunchDX11(args);
				this.Close();
			} else {
				try {
					if (!Helpers.checkDXR()) {
						Launcher.LaunchDX11(args);
						this.Close();
					}
				} catch (Exception ex) {
					Debug.WriteLine(ex);
					MessageBox.Show("Something went wrong.", "General Error", MessageBoxButton.OK, MessageBoxImage.Error);
					this.Close();
				}
			}

			Button[] buttons = { this.DirectX11Button, this.DirectX12Button };
			var lastButtonChoice = Properties.Settings.Default.LastButtonChoice;
			if (lastButtonChoice < 0)
				return;

			buttons[lastButtonChoice].IsDefault = true;
		}

		private void MainWindow_OnMouseDown(object sender, MouseButtonEventArgs e) {
			if (e.ChangedButton == MouseButton.Left)
				this.DragMove();
		}

		private void LaunchDX11_Click(object sender, RoutedEventArgs e) {
			Properties.Settings.Default.LastButtonChoice = 0;
			Launcher.LaunchDX11(Environment.CommandLine.Split());
			this.Close();
		}

		private void LaunchDX12_Click(object sender, RoutedEventArgs e) {
			Properties.Settings.Default.LastButtonChoice = 1;
			Launcher.LaunchDX12(Environment.CommandLine.Split());
			this.Close();
		}

		private void Minimize_Click(object sender, RoutedEventArgs e) {
			this.WindowState = WindowState.Minimized;
		}

		private void Exit_Click(object sender, RoutedEventArgs e) {
			this.Close();
		}

		private void MainWindow_OnClosing(object sender, CancelEventArgs e) {
			Properties.Settings.Default.Save();
		}
	}
}