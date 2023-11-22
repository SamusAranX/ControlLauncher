using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace ControlLauncher;

internal sealed class ControllerManager : IDisposable
{
	private readonly CancellationTokenSource _cts = new();
	private bool _initialized;

	private const int MAX_CONTROLLER_INDEX = 4;
	private const int SLEEP_TIME = 1000 / 60;

	[DllImport("XInputWrapper.dll")]
	private static extern bool XInputWrapper_init();

	[DllImport("XInputWrapper.dll")]
	private static extern bool XInputWrapper_updateControllerState(int index);

	[DllImport("XInputWrapper.dll")]
	private static extern bool XInputWrapper_wasAPressed(int index);

	[DllImport("XInputWrapper.dll")]
	private static extern bool XInputWrapper_wasDPadUpPressed(int index);

	[DllImport("XInputWrapper.dll")]
	private static extern bool XInputWrapper_wasDPadDownPressed(int index);

	public event ButtonPressedEventHandler? ButtonPressed;

	public void Dispose()
	{
		this._cts.Dispose();
	}

	public bool Initialize()
	{
		try
		{
			if (!XInputWrapper_init())
				return false;
		}
		catch (DllNotFoundException)
		{
			return false;
		}

		this._initialized = true;
		return true;
	}

	public void Start()
	{
		if (!this._initialized)
		{
			Debug.WriteLine("ControllerManager has not been initialized!");
			return;
		}

		ThreadPool.QueueUserWorkItem(this.Update, this._cts.Token);
	}

	public void Stop()
	{
		if (!this._initialized)
		{
			Debug.WriteLine("ControllerManager has not been initialized!");
			return;
		}

		this._cts.Cancel();
	}

	private void Update(object? cts)
	{
		if (cts is null)
			return;

		var token = (CancellationToken)cts;

		while (true)
		{
			if (token.IsCancellationRequested)
				break;

			for (var i = 0; i < MAX_CONTROLLER_INDEX; i++)
			{
				if (!XInputWrapper_updateControllerState(i))
					continue;

				if (XInputWrapper_wasAPressed(i))
				{
					this.OnButtonPressed(new ButtonPressedEventArgs(GamepadButton.ButtonA));
					break;
				}

				if (XInputWrapper_wasDPadDownPressed(i))
				{
					this.OnButtonPressed(new ButtonPressedEventArgs(GamepadButton.ButtonDPadDown));
					break;
				}

				if (XInputWrapper_wasDPadUpPressed(i))
				{
					this.OnButtonPressed(new ButtonPressedEventArgs(GamepadButton.ButtonDPadUp));
					break;
				}
			}

			Thread.Sleep(SLEEP_TIME);
		}
	}

	internal void OnButtonPressed(ButtonPressedEventArgs e)
	{
		if (this.ButtonPressed == null)
			return;

		foreach (var @delegate in this.ButtonPressed.GetInvocationList())
		{
			var invocation = (ButtonPressedEventHandler)@delegate;

			try
			{
				if (invocation.Target is ISynchronizeInvoke { InvokeRequired: true } target)
					target.Invoke(invocation, new object[] { this, e });
				else
					invocation(this, e);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}
		}
	}

	public delegate void ButtonPressedEventHandler(object sender, ButtonPressedEventArgs e);
}

public enum GamepadButton
{
	ButtonA,
	ButtonDPadUp,
	ButtonDPadDown,
}

public class ButtonPressedEventArgs : EventArgs
{
	public GamepadButton ButtonPressed;

	public ButtonPressedEventArgs(GamepadButton btn)
	{
		this.ButtonPressed = btn;
	}
}
