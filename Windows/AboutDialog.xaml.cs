using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace ControlLauncher.Windows;

public partial class AboutDialog
{
	public AboutDialog()
	{
		this.InitializeComponent();
	}

	private void AboutDialog_OnLoaded(object sender, RoutedEventArgs e)
	{
		var akzidenz = (Application.Current as App)?.AkzidenzGrotesk;
		if (akzidenz != null)
			this.FontFamily = akzidenz;

		var version = Assembly.GetExecutingAssembly().GetName().Version;
		if (version != null)
		{
			var versionString = version.ToString().Trim('0', '.');
			this.Title = $"About (ControlLauncher v{versionString})";
		}
	}

	private void Close_Click(object sender, RoutedEventArgs e)
	{
		this.Close();
	}

	private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Left)
			this.DragMove();
	}
}
