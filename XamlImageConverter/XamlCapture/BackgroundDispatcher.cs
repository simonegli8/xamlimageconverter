using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Threading;
using System.Diagnostics;

namespace XamlImageConverter.XamlCapture {

	public class BackgroundStaDispatcher {
		private Dispatcher _dispatcher;

		public BackgroundStaDispatcher(string name) {
			AutoResetEvent are = new AutoResetEvent(false);

			Thread thread = new Thread((ThreadStart)delegate {
				_dispatcher = Dispatcher.CurrentDispatcher;
				_dispatcher.UnhandledException +=
				delegate(
					  object sender,
					  DispatcherUnhandledExceptionEventArgs e) {
					if (!Debugger.IsAttached) {
						e.Handled = true;
					}
				};
				are.Set();
				Dispatcher.Run();
			});

			thread.Name = string.Format("BackgroundStaDispatcher({0})", name);
			thread.SetApartmentState(ApartmentState.STA);
			thread.IsBackground = true;
			thread.Start();

			are.WaitOne();
		}


		public void Invoke(Action action) {
			_dispatcher.Invoke(action);
		}


		public void BeginInvoke(Action action) {
			_dispatcher.BeginInvoke(action);
		}
	}
}
