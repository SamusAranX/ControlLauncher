using System.Diagnostics;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace ControlLauncher.Styles;

internal sealed class BetterHyperlink : Hyperlink
{
	public BetterHyperlink()
	{
		this.RequestNavigate += BetterHyperlink_RequestNavigate;
	}

	private static void BetterHyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
	{
		Process.Start(new ProcessStartInfo(e.Uri.ToString()) {UseShellExecute = true});
	}
}
