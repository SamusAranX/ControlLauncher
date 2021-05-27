﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ControlLauncher.Styles;

namespace ControlLauncher {
	public partial class MainWindow {

		private readonly ControllerManager controllerManager;
		private MenuButton[] allButtons;

		public MainWindow() {
			this.InitializeComponent();

			this.controllerManager = new ControllerManager();
			if (!this.controllerManager.Initialize()) {
				Debug.WriteLine("Controller initialization failed!");
				return;
			}
			this.controllerManager.Start();
			this.controllerManager.ButtonPressed += this.ControllerButtonPressed;
		}

		private void MainWindow_OnLoaded(object sender, RoutedEventArgs e) {
			var akzidenz = (App.Current as App)?.AkzidenzGrotesk;
			if (akzidenz != null)
				this.FontFamily = akzidenz;

			// Don't do all these additional things if the launcher is not in the game directory
			if (!Helpers.IsLauncherInGameDir())
				return;

			var args = Helpers.GetCommandLineArgs();
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

			if (!Helpers.CheckForRedistributable()) {
				MessageBox.Show("Microsoft Visual C++ Redistributable installation failed.", "Installation failed", MessageBoxButton.OK, MessageBoxImage.Error);
				Application.Current.Shutdown(1);
			} else if (Helpers.IsWin7OrWin8()) {
				Launcher.LaunchDX11(args);
				this.Close();
			} else {
				if (!Helpers.CheckForRaytracing()) {
					Launcher.LaunchDX11(args);
					this.Close();
				}
			}

			this.allButtons = new MenuButton[] { this.DirectX11Button, this.DirectX12Button, this.ExitButton };
			var lastButtonChoice = Properties.Settings.Default.LastButtonChoice;
			if (lastButtonChoice >= 0) {
				this.allButtons[lastButtonChoice].IsDefault = true;
				this.allButtons[lastButtonChoice].Focus();
			} else {
				this.allButtons[0].Focus();
			}
		}

		private void MainWindow_OnClosing(object sender, CancelEventArgs e) {
			this.controllerManager.Stop();
			Properties.Settings.Default.Save();
		}

		private void MainWindow_OnMouseDown(object sender, MouseButtonEventArgs e) {
			if (e.ChangedButton == MouseButton.Left)
				this.DragMove();
		}

		private void ControllerButtonPressed(object sender, ButtonPressedEventArgs e) {
			static int mod(int x, int m) {
				var r = x % m;
				return r < 0 ? r + m : r;
			}

			Application.Current.Dispatcher.Invoke(() => {
				var focusedButton = Keyboard.FocusedElement as MenuButton ?? this.allButtons.First(b => b.IsFocused);
				var focusedButtonTag = int.Parse((string)focusedButton.Tag);
				var newFocusedButton = -1;

				Debug.WriteLine($"Focused Button: {focusedButton.Tag}");

				switch (e.ButtonPressed) {
					case GamepadButton.ButtonA:
						Debug.WriteLine("Click!");
						focusedButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
						Debug.WriteLine("Clicked!");
						return;

					case GamepadButton.ButtonDPadDown:
						Debug.WriteLine("Down!");
						newFocusedButton = mod(focusedButtonTag + 1, 3);
						break;

					case GamepadButton.ButtonDPadUp:
						Debug.WriteLine("Up!");
						newFocusedButton = mod(focusedButtonTag - 1, 3);
						break;

					default:
						return;
				}

				this.allButtons[newFocusedButton].Focus();
			});
		}

		private void LaunchDX11_Click(object sender, RoutedEventArgs e) {
			Properties.Settings.Default.LastButtonChoice = 0;
			Launcher.LaunchDX11(Helpers.GetCommandLineArgs());
			this.Close();
		}

		private void LaunchDX12_Click(object sender, RoutedEventArgs e) {
			Properties.Settings.Default.LastButtonChoice = 1;
			Launcher.LaunchDX12(Helpers.GetCommandLineArgs());
			this.Close();
		}

		private void Info_Click(object sender, RoutedEventArgs e) {
			var aboutDialog = new AboutDialog {Owner = this};
			aboutDialog.ShowDialog();
		}

		private void Minimize_Click(object sender, RoutedEventArgs e) {
			this.WindowState = WindowState.Minimized;
		}

		private void Exit_Click(object sender, RoutedEventArgs e) {
			this.Close();
		}
	}
}