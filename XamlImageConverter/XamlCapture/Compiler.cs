using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using System.IO.Pipes;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Windows.Interop;
using System.Windows.Threading;

namespace XamlImageConverter {
	//TODO: ImageMaps

	public class CompilerInnerException: Exception {
		public int ErrorCode;
		public XObject XObject;
		public CompilerInnerException(string msg, int code, XObject xobj, Exception innerException) : base(msg, innerException) { ErrorCode = code; XObject = xobj; }
	}

	public class CompilerException: Exception {
		public int ErrorCode;
		public XObject XObject;
		public CompilerException(string msg, int code, XObject xobj) : base(msg) { ErrorCode = code; XObject = xobj; }
	}

	public interface ICompiler {
		List<string> SourceFiles { get; set; }
		string ProjectPath { get; set; }
		string LibraryPath { get; set; }
		bool RebuildAll { get; set; }
		bool UseService { get; set; }
		bool SeparateAppDomain { get; set; }
		CultureInfo Culture { get; set; }
		Dictionary<string, string> Parameters { get; set; }
		List<ILogger> Loggers { get; }
		void Compile();
	}

	[Serializable]
	public class Compiler: Parser, ICompiler {

		public List<string> SourceFiles { get; set; }

		public string ProjectPath { get; set; }
		public string LibraryPath { get; set; }
		public string SkinPath { get; set; }
		//public bool STAThread { get; set; }
		const bool STAThread = true;
		public bool NeedsBuilding { get; set; }
		public bool CheckBuilding { get; set; }
		public bool NeedsBuildingChecked { get; set; }
		public bool SeparateAppDomain { get; set; }
		public bool RebuildAll { get; set; }
		public bool UseService { get; set; }
		public CultureInfo Culture { get; set; }
		public Dictionary<string, string> Parameters { get; set; }
		static int id = 0;
		public List<Step> Steps { get; set; }
		public List<Process> Processes { get; set; }

		public void FinishWork() { foreach (var p in Processes.Where(p => !p.HasExited)) p.WaitForExit(); } 

		public Compiler() {
			NeedsBuilding = true; CheckBuilding = false; SeparateAppDomain = true; NeedsBuildingChecked = false;
			RebuildAll = false; UseService = false;
			SourceFiles = new List<string>();
			Parameters = new Dictionary<string, string>();
			Processes = new List<Process>();
		}

		public Compiler(string projectPath, string libraryPath): this() { ProjectPath = projectPath; LibraryPath = libraryPath; }

		public void CopyTo(Compiler dest) {
			dest.SourceFiles = SourceFiles.ToList();
			dest.ProjectPath = ProjectPath; dest.LibraryPath = LibraryPath; dest.SkinPath = SkinPath; //dest.STAThread = STAThread;
			dest.NeedsBuilding = NeedsBuilding; dest.CheckBuilding = CheckBuilding; dest.NeedsBuildingChecked = NeedsBuildingChecked;
			dest.SeparateAppDomain = SeparateAppDomain;	dest.RebuildAll = RebuildAll; dest.UseService = UseService;
			dest.Culture = Culture;
			foreach (var key in Parameters.Keys) dest.Parameters.Add(key, Parameters[key]); 
			dest.Errors.Loggers = Errors.Loggers;
			//dest.Errors = Errors;
			dest.Initialized = this.Initialized;
		}

		public List<ILogger> Loggers { get { return Errors.Loggers; } } 

		public string ID {
			get {
				var hash = id++;
				hash += ProjectPath.GetHashCode();
				hash += LibraryPath.GetHashCode();
				hash += SkinPath.GetHashCode();
				foreach (var src in SourceFiles) hash += src.GetHashCode();
				return hash.ToString();
			}
		}

 		public string MapPath(string path) {
			if (path == null) path = "";
			path = path.Replace('/', Path.DirectorySeparatorChar);
			if (path.StartsWith("~" + Path.DirectorySeparatorChar) && ProjectPath != null) {
				string project = ProjectPath;
				if (project.EndsWith(Path.DirectorySeparatorChar.ToString())) project = project.Substring(0, project.Length-1);
				return Path.Combine(project, path.Substring(2));
			} else if (path == "~") {
				return ProjectPath;
			} else if (!path.Contains(":")) {
				path = Path.Combine(SkinPath, path);
			}
			return new DirectoryInfo(path).FullName;
		}

		public void MapType(string path, out string assemblyName, out string typeName, out string assemblyQualifiedName) {
			string[] tokens = path.Split(',');
			if (tokens.Length > 1) {
				assemblyName = MapPath(tokens[1].Trim().Replace("\\\\", "\\"));
				tokens[1] = " " + Path.GetFileNameWithoutExtension(assemblyName);
				assemblyQualifiedName = string.Join(",", tokens);
				typeName = tokens[0];
			} else {
				assemblyName = null;
				assemblyQualifiedName = null;
				typeName = path;
			}	
		}

		bool CheckNeedsBuilding(IEnumerable<Group> scenes) {
			if (NeedsBuildingChecked) return true; 
			foreach (Group scene in scenes) {
				foreach (var step in scene.Steps()) {
					if (step.NeedsBuilding) {
						NeedsBuildingChecked = true;
						return true;
					}
				}
			}
			return false;
		}

		[NonSerialized]
		bool init= false;
		[NonSerialized]
		static bool dllsLoaded = false;
		[NonSerialized]
		public static object DllLock = new object();

		public bool Initialized { get { return init; } set { init = value; } }

		void Init() {
			if (!init) {
				init = true;
				Errors.Message("XamlImageConverter 3.4 by Chris Cavanagh & David Egli");
			}
		}

		void LoadDlls() {
			lock (DllLock) {
				if (dllsLoaded) return;
				if (!string.IsNullOrEmpty(LibraryPath)) {
					var path = Path.Combine(ProjectPath, LibraryPath);
					if (!string.IsNullOrEmpty(path)) {
						var files = Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories);
						foreach (var file in files) {
							try {
								var pdb = Path.ChangeExtension(file, "pdb");
								Assembly assembly;
								//if (File.Exists(pdb)) assembly = Assembly.Load(File.ReadAllBytes(file), File.ReadAllBytes(pdb));
								//else
								assembly = Assembly.LoadFrom(file);

								if (assembly != null) Errors.Message("Assembly {0} loaded.", Path.GetFileNameWithoutExtension(file));
							} catch { }
						}
					}
				}
				dllsLoaded = true;
			}
		}

		/*
		void RawCompile(IEnumerable<string> SourceFiles, string ProjectPath, string LibraryPath) {
	
			foreach (var src in 
			Compile(SourceFiles, RebuildAll, ProjectPath, LibraryPath, true, true, false);
		}
		*/

		void Compile(string filename) {
			Init();
			Errors.Path = filename;
			if (string.IsNullOrEmpty(ProjectPath)) ProjectPath = Path.GetDirectoryName(filename);
 
			SkinPath = Path.Combine(ProjectPath, Path.GetDirectoryName(filename));

			List<string> directExtensions = new List<string> {
				".xaml.png", ".xaml.jpg", ".xaml.jpeg", ".xaml.gif", ".xaml.bmp", ".xaml.tif", ".xaml.tiff", ".xaml.pdf", ".xaml.wdp", ".xaml.eps", ".xaml.ps", ".xaml.xps",
				".svg.png", ".svg.jpg", ".svg.jpeg", "svg.gif", ".svg.bmp", ".svg.tif", "svg.tiff", "svg.pdf", ".svg.wpd", ".svg.eps", "svg.ps", ".svg.xps",
				".svgz.png", ".svgz.jpg", ".svgz.jpeg", "svgz.gif", ".svgz.bmp", ".svgz.tif", "svgz.tiff", "svgz.pdf", ".svgz.wpd", ".svgz.eps", "svgz.ps", ".svgz.xps",
				".psd.png", ".psd.jpg", ".psd.jpeg", "psd.gif", ".psd.bmp", ".psd.tif", "psd.tiff", "psd.pdf", ".psd.wpd", ".psd.eps", "psd.ps", ".psd.xps"
			};

			XElement config = null;
			DateTime Version = DateTime.Now.ToUniversalTime();
			try {
				if (!RebuildAll) {
					FileInfo info = new FileInfo(filename);
					Version = info.LastWriteTimeUtc;
				}
				var lowername = filename.ToLower();
				if (filename.Trim()[0] == '#') filename = (string)System.Web.HttpContext.Current.Session["XamlImageConverter.Xaml:" + filename];
				if (filename.Trim()[0] == '<') {
					using (var r = new StringReader(filename)) {
						var xdoc = XElement.Load(r, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);
						if (xdoc.Name.LocalName == "XamlImageConverter" && (xdoc.Name.NamespaceName == Parser.ns1 || xdoc.Name.NamespaceName == Parser.ns2)) config = xdoc;
						else config = XamlScene.CreateDirect(xdoc, Parameters);
					}
				} else if (directExtensions.Any(x => lowername.EndsWith(x)) || (lowername.EndsWith(".xaml") && !lowername.EndsWith(".xic.xaml"))) config = XamlScene.CreateDirect(filename, Parameters);
				else if (filename == "XamlImageConverter.axd") {
					config = XamlScene.CreateXsd(Parameters);
				} else {
					config = XElement.Load(filename, LoadOptions.SetBaseUri | LoadOptions.SetLineInfo | LoadOptions.PreserveWhitespace);
				}
			} catch (FileNotFoundException) {
				Errors.Error("Unable to read the configuration file", "1", null);
				return;
			} catch (Exception ex) {
				Errors.Error(ex.Message, "21", null);
				return;
			}
			Compile(Version, config);
		}

		void Compile(DateTime Version, XElement config) {

			Step lastStep = null;
			if (RebuildAll) Version = DateTime.Now.ToUniversalTime();
			try {
				var scenes = ParseScenes(Version, config).ToList();

				var building = CheckNeedsBuilding(scenes);
				
				NeedsBuilding = NeedsBuilding || building;

				// compile dependencies
				foreach (var scene in scenes) {
					foreach (var dependency in scene.DependsOn) {
						var c = new Compiler();
						this.CopyTo(c);
						c.SourceFiles = new List<string>() { dependency };
						c.SeparateAppDomain = true;
						c.Compile();
					}
				}
		
				if (!building || CheckBuilding) return;

				LoadDlls();
				// Get flattened list of snapshots
				// Could just return enumerator and parse + save per iteration, but pre-populating with ToList()
				// means we can throw parsing errors etc before any snapshots are saved (so the user doesn't have
				// to wait as long to see potential errors)
				Steps = scenes.SelectMany(scene => scene.Steps()).ToList();

				foreach (var step in Steps) {
					lastStep = step;
					//TODO collect?
					//System.GC.Collect(System.GC.MaxGeneration, GCCollectionMode.Forced);
					step.Process();
				}
				// flush Dispatcher
				//Dispatcher.CurrentDispatcher.Invoke((Action)(() => {}), DispatcherPriority.ApplicationIdle);
				Group.Close(Processes, Errors, scenes);
			} catch (CompilerException cex) {
				Errors.Error(cex.Message, cex.ErrorCode.ToString(), cex.XObject);
			} catch (Exception ex) {
				XObject xobj = null;
				if (lastStep != null && lastStep is Group) xobj = ((Group)lastStep).XElement;
				Errors.Warning("An internal error occurred\n\n" + ex.Message + "\n" + ex.StackTrace, "2", xobj);
			}
			lastStep = null;
			System.GC.Collect(System.GC.MaxGeneration, GCCollectionMode.Forced);
		}

		void RawCompile() {
			/*
			var dispatcher = Dispatcher.FromThread(Thread.CurrentThread);
			if (dispatcher == null) {
				AutoResetEvent are = new AutoResetEvent(false);

				Thread thread = new Thread((ThreadStart)delegate {
					var d = Dispatcher.CurrentDispatcher;
					d.UnhandledException += delegate(object sender, DispatcherUnhandledExceptionEventArgs e) {
						if (!Debugger.IsAttached) e.Handled = true;
					};
					are.Set();
					Dispatcher.Run();
				});

				thread.Name = "BackgroundStaDispatcher";
				thread.SetApartmentState(ApartmentState.STA);
				thread.IsBackground = true;
				thread.Start();

				are.WaitOne();
			} */
			foreach (var file in SourceFiles) Compile(file);
		}

		void CoreCompile() {
			if (STAThread && Thread.CurrentThread.GetApartmentState() != ApartmentState.STA) {
				Thread thread = new Thread(() => { RawCompile(); });
				thread.SetApartmentState(ApartmentState.STA);
				if (Culture != null) { thread.CurrentCulture = thread.CurrentUICulture = Culture; }
				thread.Start();
				thread.Join();
			} else {
				if (Culture != null) {
					var culture = Thread.CurrentThread.CurrentCulture;
					var uiculture = Thread.CurrentThread.CurrentUICulture;
					Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = Culture;
					RawCompile();
					Thread.CurrentThread.CurrentCulture = culture;
					Thread.CurrentThread.CurrentUICulture = uiculture;
				} else {
					RawCompile();
				}
			}
		}


		public long MemorySet { get { return GC.GetTotalMemory(false); } }

		public void Compile() {
			if (SourceFiles != null && SourceFiles.Count > 0) {
				if (!ProjectPath.Contains(":")) { ProjectPath = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, ProjectPath)).FullName; }
					
				if (UseService) {
					var server = new CompilerServer();
					server.Compile(this);
				} else if (SeparateAppDomain) {
					NeedsBuilding = false;
					CheckBuilding = true;
					CoreCompile();
					CheckBuilding = false;
					if (NeedsBuilding) {
						AppDomainSetup setup = new AppDomainSetup();
						setup.ApplicationBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
						var domain = AppDomain.CreateDomain("XamlImageConverter Compiler", null, setup);
						try {
							Compiler compiler = (Compiler)domain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, "XamlImageConverter.Compiler");
							CopyTo(compiler);
							compiler.SeparateAppDomain = false;
							//compiler.STAThread = true;
							compiler.Compile();
						} catch {
						} finally {
							AppDomain.Unload(domain);
							GC.Collect(10, GCCollectionMode.Forced);
							GC.WaitForFullGCComplete(1000);
						}
					}
				} else {
					CoreCompile();
				}

				FinishWork();

				foreach (var logger in Errors.Loggers.OfType<IDisposable>()) logger.Dispose();
			}
		}

		public void Compile(IEnumerable<string> SourceFiles) { this.SourceFiles = SourceFiles.ToList(); Compile(); }
	}

}
