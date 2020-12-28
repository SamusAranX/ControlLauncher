using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using ControlLauncher.RMDP;
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

		private void LoadFonts() {
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
			}
		}

		private void MainWindow_OnLoaded(object sender, RoutedEventArgs e) {
			this.LoadFonts();

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

			if (!Helpers.CheckForCDll()) {
				MessageBox.Show("Microsoft Visual C++ Redistributable installation failed.", "Installation failed", MessageBoxButton.OK, MessageBoxImage.Error);
				this.Close();
			} else if (Helpers.IsWin7OrWin8()) {
				Launcher.LaunchDX11(args);
				this.Close();
			} else {
				try {
					if (!Helpers.CheckDXR()) {
						Launcher.LaunchDX11(args);
						this.Close();
					}
				} catch (Exception ex) {
					Debug.WriteLine(ex);
					MessageBox.Show("Something went wrong.", "General Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
			int mod(int x, int m) {
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

		private void Minimize_Click(object sender, RoutedEventArgs e) {
			this.WindowState = WindowState.Minimized;
		}

		private void Exit_Click(object sender, RoutedEventArgs e) {
			this.Close();
		}
	}
}