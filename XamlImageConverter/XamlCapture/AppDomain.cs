using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace XamlImageConverter {
	public class AppDomains: List<AppDomains.AppDomain> {

		public class AppDomain {
			static int N = 0;

			System.AppDomain Domain = null;
			public List<Thread> Threads = new List<Thread>();
			
			public AppDomain() {
				AppDomainSetup setup = new AppDomainSetup();
				setup.ApplicationBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				Domain = AppDomain.CreateDomain("XamlImageConverter Compiler " + ++N.ToString(), null, setup);
			}

			public bool IsShuttingDown;

			public Thread CreateThread() {
			}

			public void Shutdown() {
				lock (this) {
					IsShuttingDown = true;
					foreach (var thread in Threads) thread.Join();

				}
			}

		}
		List<Compiler> compilers = new List<Compiler>();
		List<Thread> Threads;

		public void Shutdown() {

			foreach (var thread in Threads) { 
			}
		}

		public void Compile(IEnumerable<string> files, bool rebuildAll, string projectPath, string libraryPath, bool createAppDomain, bool STAThread) {
			if (files != null && files.Count() > 0) {
				var compiler = new Compiler();
				compiler.SourceFiles = files.ToList();
				if (!projectPath.Contains(":")) { projectPath = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, projectPath)).FullName; }
				compiler.ProjectPath = projectPath;
				compiler.LibraryPath = libraryPath;

				if (createAppDomain) {
					if (domain == null) NewDomain();
					Compiler compiler = (Compiler)ad.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, "XamlImageConverter.Compiler");
					compiler.RemoteCompile(files, rebuildAll, projectPath, libraryPath, STAThread, Errors.Logger);
					
				} else {
					if (STAThread && Thread.CurrentThread.GetApartmentState() != ApartmentState.STA) {
						Thread thread = new Thread(delegate() {
							foreach (var file in files) Compile(file, rebuildAll);
						});
						thread.SetApartmentState(ApartmentState.STA);
						lock (Threads) {
							Threads.Add(thread);
							thread.Start();
						}
						thread.Join();
					} else {
						foreach (var file in files) Process(file, rebuildAll, Compile);
					}
				}
				if (Errors.Logger != null && Errors.Logger is IDisposable) {
					((IDisposable)Errors.Logger).Dispose();
				}
			}
		}

		[STAThread]
		private void RemoteCompile(IEnumerable<string> files, bool rebuildAll, string projectPath, string libraryPath, bool STAThread, ILogger logger) {
			Errors.Logger = logger;
			Thread thread = new Thread(delegate() {
				Compile(files, rebuildAll, projectPath, libraryPath, false, STAThread);
			});
			thread.SetApartmentState(ApartmentState.STA);
			lock (Threads) {
				Threads.Add(thread);
				thread.Start();
			}
			thread.Join();
		}

	}
}
