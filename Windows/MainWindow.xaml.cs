using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ControlLauncher.Styles;

namespace ControlLauncher.Windows;

public partial class MainWindow : IDisposable
{
	private readonly ControllerManager _controllerManager;
	private MenuButton[] _allButtons = Array.Empty<MenuButton>();

	public MainWindow()
	{
		this.InitializeComponent();

		this._controllerManager = new ControllerManager();
		if (!this._controllerManager.Initialize())
		{
			Debug.WriteLine("Controller initialization failed!");
			return;
		}

		this._controllerManager.Start();
		this._controllerManager.ButtonPressed += this.ControllerButtonPressed;
	}

	public void Dispose()
	{
		this._controllerManager.Dispose();
	}

	private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
	{
		var akzidenz = (Application.Current as App)?.AkzidenzGrotesk;
		if (akzidenz != null)
			this.FontFamily = akzidenz;

		// Don't do all these additional things if the launcher is not in the game directory
		if (!Helpers.IsLauncherInGameDir())
			return;

		var args = Helpers.GetCommandLineArgs();
		if (args.Contains("-dx11", StringComparer.OrdinalIgnoreCase))
		{

			Launcher.LaunchDX11(args);
			this.Close();
			return;
		}

		if (args.Contains("-dx12", StringComparer.OrdinalIgnoreCase))
		{
			Launcher.LaunchDX12(args);
			this.Close();
			return;
		}

		if (!Helpers.InstallRedistributableIfNeeded())
		{
			MessageBox.Show("Microsoft Visual C++ Redistributable installation failed.", "Installation failed", MessageBoxButton.OK, MessageBoxImage.Error);
			Application.Current.Shutdown(1);
		}
		else if (Helpers.IsWin7OrWin8())
		{
			Launcher.LaunchDX11(args);
			this.Close();
		}
		else
		{
			if (!Helpers.CheckForRaytracing())
			{
				Launcher.LaunchDX11(args);
				this.Close();
			}
		}

		this._allButtons = new[] { this.DirectX11Button, this.DirectX12Button, this.ExitButton };
		var lastButtonChoice = Properties.Settings.Default.LastButtonChoice;
		if (lastButtonChoice >= 0)
		{
			this._allButtons[lastButtonChoice].IsDefault = true;
			this._allButtons[lastButtonChoice].Focus();
		}
		else
			this._allButtons[0].Focus();
	}

	private void MainWindow_OnClosing(object sender, CancelEventArgs e)
	{
		this._controllerManager.Stop();
		Properties.Settings.Default.Save();
	}

	private void MainWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Left)
			this.DragMove();
	}

	private void ControllerButtonPressed(object sender, ButtonPressedEventArgs e)
	{
		Application.Current.Dispatcher.Invoke(() =>
		{
			var focusedButton = Keyboard.FocusedElement as MenuButton ?? this._allButtons.First(b => b.IsFocused);
			var focusedButtonTag = int.Parse((string)focusedButton.Tag, NumberStyles.Integer, CultureInfo.InvariantCulture);
			int newFocusedButton;

			Debug.WriteLine($"Focused Button: {focusedButton.Tag}");

			switch (e.ButtonPressed)
			{
				case GamepadButton.ButtonA:
					Debug.WriteLine("Click!");
					focusedButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
					Debug.WriteLine("Clicked!");
					return;

				case GamepadButton.ButtonDPadDown:
					Debug.WriteLine("Down!");
					newFocusedButton = Mod(focusedButtonTag + 1, 3);
					break;

				case GamepadButton.ButtonDPadUp:
					Debug.WriteLine("Up!");
					newFocusedButton = Mod(focusedButtonTag - 1, 3);
					break;

				default:
					return;
			}

			this._allButtons[newFocusedButton].Focus();
		});
		return;

		static int Mod(int x, int m)
		{
			var r = x % m;
			return r < 0 ? r + m : r;
		}
	}

	private void LaunchDX11_Click(object sender, RoutedEventArgs e)
	{
		Properties.Settings.Default.LastButtonChoice = 0;
		Launcher.LaunchDX11(Helpers.GetCommandLineArgs());
		this.Close();
	}

	private void LaunchDX12_Click(object sender, RoutedEventArgs e)
	{
		Properties.Settings.Default.LastButtonChoice = 1;
		Launcher.LaunchDX12(Helpers.GetCommandLineArgs());
		this.Close();
	}

	private void Info_Click(object sender, RoutedEventArgs e)
	{
		var aboutDialog = new AboutDialog { Owner = this };
		aboutDialog.ShowDialog();
	}

	private void Minimize_Click(object sender, RoutedEventArgs e)
	{
		this.WindowState = WindowState.Minimized;
	}

	private void Exit_Click(object sender, RoutedEventArgs e)
	{
		this.Close();
	}
}
