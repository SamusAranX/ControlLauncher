using System.Diagnostics;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace ControlLauncher.Styles {
	class BetterHyperlink : Hyperlink {

		public BetterHyperlink() {
			this.RequestNavigate += this.BetterHyperlink_RequestNavigate;
		}

		private void BetterHyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) {
			Process.Start(e.Uri.ToString());
		}
	}
}
