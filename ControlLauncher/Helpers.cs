using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace ControlLauncher {
	internal class Helpers {
		[DllImport("CheckDXR.dll")]
		public static extern bool checkDXR();

		[DllImport("CheckCDLL.dll")]
		private static extern bool checkCDLL();

		public static bool IsWin7OrWin8() {
			var productName = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion")?.GetValue("ProductName").ToString();
			if (productName == null)
				return false;

			return productName.Contains("Windows 7") || productName.Contains("Windows 8");
		}

		public static bool CheckForCDll() {
			var vcInstallAttempted = false;

			try {
				checkCDLL();
			} catch (Exception) {
				Debug.WriteLine("VC++ Redistributable check failed!");
#if !DEBUG
				var vcProcess = Process.Start("vc_redist.x64.exe", "/install /norestart");
				if (vcProcess == null)
					return false;

				vcProcess.WaitForExit();
				vcInstallAttempted = true;
#endif
			}

			if (!vcInstallAttempted)
				return true;

			try {
				checkCDLL();
			} catch (Exception) {
				return false;
			}

			return true;
		}
	}
}