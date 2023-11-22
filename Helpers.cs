using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace ControlLauncher;

internal sealed class Helpers
{
	[DllImport("CheckDXR.dll", EntryPoint = "checkDXR")]
	private static extern bool CheckDXR();

	[DllImport("CheckCDLL.dll", EntryPoint = "checkCDLL")]
	private static extern bool CheckCDLL();

	[DllImport("shell32.dll", SetLastError = true)]
	private static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

#if DEBUG
	public const bool DEBUG = true;
#else
	public const bool DEBUG = false;
#endif

	/// <summary>
	/// Checks the OS version and returns true if it's Windows 7 or 8/8.1. See: <see href="https://learn.microsoft.com/en-us/windows/win32/sysinfo/operating-system-version">Operating System Version</see>
	/// </summary>
	/// <returns>True if the current OS is Windows 7 or 8/8.1, false otherwise.</returns>
	public static bool IsWin7OrWin8()
	{
		return Environment.OSVersion.Version is { Major: 6, Minor: >= 1 and <= 3 };
	}

	private static bool CheckRedistributable()
	{
		try
		{
			return CheckCDLL();
		}
		catch (DllNotFoundException)
		{
			return false;
		}
	}

	public static bool CheckForRaytracing()
	{
		try
		{
			return CheckDXR();
		}
		catch (DllNotFoundException)
		{
			return false;
		}
	}

	/// <summary>
	/// Checks whether the VC++ redistributable is installed and launches the installer if needed.
	/// </summary>
	/// <returns>True if the redistributable is installed, false otherwise or if the installer failed.</returns>
	public static bool InstallRedistributableIfNeeded()
	{
		var vcInstallAttempted = false;

		if (CheckRedistributable())
			return true;

		if (!DEBUG)
		{
			vcInstallAttempted = true;
			try
			{
				var vcProcess = Process.Start("VC_redist.x64.exe", "/install /norestart");
				if (vcProcess == null)
					return false;

				vcProcess.WaitForExit();
			}
			catch (Exception e)
			{
				return false;
			}
		}

		if (!vcInstallAttempted)
			return true;

		return CheckRedistributable();
	}

	public static bool IsLauncherInGameDir()
	{
		try
		{
			CheckDXR();
			CheckCDLL();
		}
		catch (DllNotFoundException)
		{
			return false;
		}

		return true;
	}

	private static IEnumerable<string> CommandLineToArgs(string commandLine)
	{
		var argv = CommandLineToArgvW(commandLine, out var argc);
		if (argv == IntPtr.Zero)
			return Array.Empty<string>();

		try
		{
			var args = new string[argc];
			for (var i = 0; i < args.Length; i++)
			{
				var p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
				args[i] = Marshal.PtrToStringUni(p)!;
			}

			return args;
		}
		finally
		{
			Marshal.FreeHGlobal(argv);
		}
	}

	public static string[] GetCommandLineArgs()
	{
		return CommandLineToArgs(Environment.CommandLine).Skip(1).ToArray();
	}
}
