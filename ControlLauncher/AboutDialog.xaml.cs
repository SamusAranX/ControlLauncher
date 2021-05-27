using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace ControlLauncher {
	public partial class AboutDialog : Window {
		public AboutDialog() {
			this.InitializeComponent();
		}

		private void AboutDialog_OnLoaded(object sender, RoutedEventArgs e) {
			var akzidenz = (App.Current as App)?.AkzidenzGrotesk;
			if (akzidenz != null)
				this.FontFamily = akzidenz;

			var version = Assembly.GetExecutingAssembly().GetName().Version.ToString().Trim('0', '.');
			this.Title = $"About (ControlLauncher v{version})";
		}

		private void Close_Click(object sender, RoutedEventArgs e) {
			this.Close();
		}

		private void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
			if (e.ChangedButton == MouseButton.Left)
				this.DragMove();
		}
	}
}
