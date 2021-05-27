using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace ControlLauncher {
	internal class ControllerManager {

		private Thread workerThread;

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

		public event ButtonPressedHandler ButtonPressed;

		public bool Initialize() {
			try {
				if (!XInputWrapper_init())
					return false;
			} catch (DllNotFoundException) {
				return false;
			}

			this.workerThread = new Thread(this.Update);
			return true;
		}

		public void Start() {
			this.workerThread.Start();
		}

		public void Stop() {
			this.workerThread?.Abort();
		}

		private void Update() {
			while (true) {
				for (var i = 0; i < MAX_CONTROLLER_INDEX; i++) {
					if (!XInputWrapper_updateControllerState(i)) 
						continue;

					if (XInputWrapper_wasAPressed(i)) {
						this.OnButtonPressed(new ButtonPressedEventArgs(GamepadButton.ButtonA));
						break;
					}

					if (XInputWrapper_wasDPadDownPressed(i)) {
						this.OnButtonPressed(new ButtonPressedEventArgs(GamepadButton.ButtonDPadDown));
						break;
					}

					if (XInputWrapper_wasDPadUpPressed(i)) {
						this.OnButtonPressed(new ButtonPressedEventArgs(GamepadButton.ButtonDPadUp));
						break;
					}
				}

				Thread.Sleep(SLEEP_TIME);
			}
		}

		internal void OnButtonPressed(ButtonPressedEventArgs e) {
			if (this.ButtonPressed == null)
				return;

			foreach (var @delegate in this.ButtonPressed.GetInvocationList()) {
				var invocation = (ButtonPressedHandler)@delegate;

				try {
					if (invocation.Target is ISynchronizeInvoke target && target.InvokeRequired)
						target.Invoke(invocation, new object[] { this, e });
					else
						invocation(this, e);
				} catch (Exception ex) {
					Debug.WriteLine(ex);
				}
			}
		}

		public delegate void ButtonPressedHandler(object sender, ButtonPressedEventArgs e);
	}

	public enum GamepadButton {
		ButtonA,
		ButtonDPadUp,
		ButtonDPadDown,
	}

	public class ButtonPressedEventArgs : EventArgs {
		public GamepadButton ButtonPressed;
		public ButtonPressedEventArgs(GamepadButton b) => this.ButtonPressed = b;
	}
}
