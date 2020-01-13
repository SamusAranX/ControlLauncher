using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ControlLauncher
{
	public partial class MainWindow {
		public MainWindow()
		{
			InitializeComponent();
		}

		private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
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
				} catch (Exception) {
					MessageBox.Show("Something went wrong.", "General Error", MessageBoxButton.OK, MessageBoxImage.Error);
					this.Close();
				}
			}

			Button[] buttons = { DirectX11Button, DirectX12Button };
			var lastButtonChoice = Properties.Settings.Default.LastButtonChoice;
			if (lastButtonChoice < 0)
				return;

			buttons[lastButtonChoice].IsDefault = true;
		}

		private void MainWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
				DragMove();
		}

		private void LaunchDX11_Click(object sender, RoutedEventArgs e)
		{
			Properties.Settings.Default.LastButtonChoice = 0;
			Launcher.LaunchDX11(Environment.CommandLine.Split());
			this.Close();
		}

		private void LaunchDX12_Click(object sender, RoutedEventArgs e)
		{
			Properties.Settings.Default.LastButtonChoice = 1;
			Launcher.LaunchDX12(Environment.CommandLine.Split());
			this.Close();
		}

		private void Exit_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void MainWindow_OnClosing(object sender, CancelEventArgs e)
		{
			Properties.Settings.Default.Save();
		}
	}
}