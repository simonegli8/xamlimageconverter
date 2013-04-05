using System;
using System.Linq;
using System.Xml.Linq;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.ServiceProcess;

namespace XamlImageConverter {

	public class Program {
		[STAThread]
		public static void Main(string[] args) {

			List<string> a = new List<string>(args);
			bool waitForKey = false;
			int i;

			if (a.Count == 1 && a[0] == "-x") {
				var srvc = new Service();
				srvc.Start();
			}

			if (a.Count == 0 || new[] { "-h", "help", "-help", "?", "/?", "-?" }.Any(s => a.Any(at => s == at.Trim().ToLower()))) {
				Console.WriteLine("XamlImageConverter 3.6 by Chris Cavanagh & Simon Egli");
				Console.WriteLine("Creates snapshots, gif animations or html image maps from XAML, SVG & PSD images\n\r");
				Console.Error.WriteLine("XamlImageConverter [-x] [-w] [-1] [-s [running time]] [-r] [-v]");
				Console.Error.WriteLine("   [-l librarypath] [-p projectpath] configFile { configFile }");
				Console.Error.WriteLine("A configFile is a xaml batch file, describing an image conversion job.");
				Console.Error.WriteLine("Documentation for creating a configFile can be found at");
				Console.Error.WriteLine("   http://xamlimageconverter.codeplex.com.");
				Console.Error.WriteLine("  -1 option: Use only one cpu core.");
				Console.Error.WriteLine("  -w option: Wait for key at end.");
				Console.Error.WriteLine("  -s option: Run as server for the HttpHandler.");
				Console.Error.WriteLine("             (To support 3D xaml, that doesn't render under IIS).");
				Console.Error.WriteLine("  -x option: Run as server for the HttpHandler as Windows service.");
				Console.Error.WriteLine("             (Does also not render 3D xaml).");
				Console.Error.WriteLine("  -r option: Rebuild all files.");
				Console.Error.WriteLine("  -l option: The path to a library folder. All dll's in this folder will be");
				Console.Error.WriteLine("             loaded, and classes therein can be referenced.");
				Console.Error.WriteLine("             (Usually the bin folder).");
				Console.Error.WriteLine("  -p option: The path to the project. Used to resolve app relative paths.");
				Console.Error.WriteLine("  -v option: Create logfiles.");
				Console.Error.WriteLine("  -f option: Don't use separate AppDomain for each sourcefile.");
				Console.Error.WriteLine("             Speeds up compilation but uses more memory.");
				Console.Error.WriteLine("  -? option: Show this help text.");
				a.Remove("-h");
				a.Remove("help");
				a.Remove("-help");
				a.Remove("?");
				a.Remove("/?");
				a.Remove("-?");
			} 
			
			if (a.Contains("-w")) {
				a.Remove("-w");
				waitForKey = true;
			}
			bool manycore = true;
			if (a.Contains("-1")) {
				a.Remove("-1");
				manycore = false;
			}


			bool server = false;
			bool test = false;
			bool debug = false;
			TimeSpan RunTime = new TimeSpan(0);
			if (a.Contains("-s")) {
				i = a.IndexOf("-s");
				server = true;
				a.RemoveAt(i);
				if (TimeSpan.TryParse(a[i], out RunTime)) a.RemoveAt(i);
				if (a.Contains("-t")) { test = true; a.Remove("-t"); }
				if (a.Contains("-d")) { debug = true; a.Remove("-d"); }
			}

			bool rebuildAll = false;
			if (a.Contains("-r")) {
				rebuildAll = true;
				a.Remove("-r");
			}

			bool useAppDomain = true;
			if (a.Contains("-f")) {
				useAppDomain = false;
				a.Remove("-f");
			}

			bool log = false;
			if (a.Contains("-v")) {
				log = true;
				a.Remove("-v");
			}

			string libraryPath = string.Empty;
			if (a.Contains("-l")) {
				i = a.IndexOf("-l");
				libraryPath = a[i+1];
				a.RemoveRange(i, 2);
			}

			string projectPath = string.Empty;
			if (a.Contains("-p")) {
				i = a.IndexOf("-p");
				projectPath = a[i+1];
				a.RemoveRange(i, 2);
			}

			List<string> files = new List<string>();
			foreach (var f in a) {
				var appRoot = projectPath;
					 var file = f;
				if (appRoot.EndsWith("\\")) appRoot = appRoot.Substring(0, appRoot.Length - 1);
				file = file.Replace("~", appRoot)
					.Replace("/", "\\");
				if (file.Contains("*")) {
					string dir, pattern;
					dir = Path.GetDirectoryName(file);
					pattern = Path.GetFileName(file);
					if (!dir.Contains("*")) {
						files.AddRange(Directory.GetFiles(dir, pattern));
					}
				} else {
					files.Add(file);
				}
			}

			if (server) {
				var cserver = new CompilerServer();
				if (RunTime.Ticks != 0) cserver.RunTime = RunTime;
				if (test) {
					ThreadPool.QueueUserWorkItem(delegate(object state) {
						Thread.Sleep(20);
						var compiler = new Compiler();
						compiler.LibraryPath = libraryPath;
						compiler.ProjectPath = projectPath;
						compiler.RebuildAll = rebuildAll;
						compiler.Parallel = manycore;
						if (log) compiler.Loggers.Add(new FileLogger());
						compiler.UseService = true;
						compiler.Compile(files);
						//cserver.Compile(compiler);
					});
				}
				cserver.Start();
  
			} else {
				var compiler = new Compiler();
				compiler.LibraryPath = libraryPath;
				compiler.ProjectPath = projectPath;
				compiler.RebuildAll = rebuildAll;
				compiler.Parallel = manycore;
				compiler.SeparateAppDomain = useAppDomain;
				if (log) compiler.Loggers.Add(new FileLogger());
				compiler.Compile(files);
			}

			if (waitForKey) {
				Console.WriteLine("Press any key...");
				Console.ReadKey();
			}

		}
	}
}