using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace XamlImageConverter {

	public partial class Service: ServiceBase {
		
		CompilerServer Server;

		public Service() {
			InitializeComponent();
			CanPauseAndContinue = true;
			CanShutdown = true;
			Server = new CompilerServer();
			Server.Service = true;
		}

		protected override void OnStart(string[] args) { Server.Start(); }
		protected override void OnStop() { Server.Stop(); }
		protected override void OnPause() { Server.Stop(); }
		protected override void OnContinue() { Server.Start(); }
		protected override void OnShutdown() { Server.Stop(); }

		public void Start() {
			//OnStart(new string[0]);
			Run(this);
		}
	}
}
