using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace ControlLauncher {
	internal class Helpers {
		[DllImport("CheckDXR.dll", EntryPoint = "checkDXR")]
		private static extern bool CheckDXR();

		[DllImport("CheckCDLL.dll", EntryPoint = "checkCDLL")]
		private static extern bool CheckCDLL();

		[DllImport("shell32.dll", SetLastError = true)]
		private static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

		public static bool IsWin7OrWin8() {
			var productName = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion")?.GetValue("ProductName").ToString();
			if (productName == null)
				return false;

			return productName.Contains("Windows 7") || productName.Contains("Windows 8");
		}

		public static bool CheckForRedistributable() {
			var vcInstallAttempted = false;

			if (CheckCDLL()) {
				return true;
			} else {
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

			return CheckCDLL();
		}

		public static bool CheckForRaytracing() {
			return CheckDXR();
		}

		public static bool IsLauncherInGameDir() {
			try {
				CheckDXR();
				CheckCDLL();
			} catch (DllNotFoundException) {
				return false;
			}

			return true;
		}

		private static string[] CommandLineToArgs(string commandLine) {
			var argv = CommandLineToArgvW(commandLine, out var argc);
			if (argv == IntPtr.Zero)
				return new string[] { };

			try {
				var args = new string[argc];
				for (var i = 0; i < args.Length; i++) {
					var p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
					args[i] = Marshal.PtrToStringUni(p);
				}

				return args;
			} finally {
				Marshal.FreeHGlobal(argv);
			}
		}

		public static string[] GetCommandLineArgs() {
			return Helpers.CommandLineToArgs(Environment.CommandLine).Skip(1).ToArray();
		}
	}
}