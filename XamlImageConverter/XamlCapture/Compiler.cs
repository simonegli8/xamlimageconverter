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
using System.Diagnostics.Contracts;
using System.Windows.Threading;
using System.Threading.Tasks;

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
		bool Parallel { get; set; }
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
		public bool ChildAppDomain { get; set; }
		public bool RebuildAll { get; set; }
		public bool UseService { get; set; }
		public bool Parallel { get; set; }
		public int GCLevel { get; set; }
		public Action Serve { get; set; }
		public CultureInfo Culture { get; set; }
		public Dictionary<string, string> Parameters { get; set; }
		static int id = 0;
		public List<Step> Steps { get; set; }
		public List<Process> Processes { get; set; }
		public System.Web.HttpContext Context { get; set; }  
		public int? hash;
		[NonSerialized]
		public List<string> TempPaths = new List<string>();
		[NonSerialized]
		public List<string> TempFiles = new List<string>();
		[NonSerialized]
		public static string CurrentTheme = "";
		[NonSerialized]
		public static string CurrentSkin = "";
		[NonSerialized]
		public int Cpus = 1;
		[NonSerialized]
		public int? Cores = null;
		[NonSerialized]
		int CreatedImages = 0;
		[NonSerialized]
		DateTime Start;
		[NonSerialized]
		List<Thread> Threads = new List<Thread>();
		[NonSerialized]
		public ManualResetEvent Finished = new ManualResetEvent(true);

		public void Finish() {
	
			foreach (var t in Threads) t.Join();
			Threads.Clear();
	
			IEnumerable<System.Diagnostics.Process> active;
			lock (Processes) active = Processes.ToList();
			foreach (var p in active) p.WaitForExit();
			lock (Processes) Processes.Clear();
		}

		public class FileLocks: IDisposable {
			static Dictionary<string, object> Locks = new Dictionary<string, object>();
			string path;
			bool IsLocked = false, IsBlocking = false;


			public FileLocks(string path) {
				this.path = path;
				lock (Locks) {
					if (!Locks.ContainsKey(path)) Locks.Add(path, new object());
				}
				IsBlocking = true;
				Monitor.Enter(Locks[path]);
				IsLocked = true; IsBlocking = false;
			}

			public void Dispose() {
				Monitor.Exit(Locks[path]);
				IsLocked = false;
			}
		}

		public IDisposable FileLock(string path) {
			return new FileLocks(path);
		}

		public void Cleanup() {
			Errors.Flush();

			foreach (var file in TempFiles) {
				System.IO.File.Delete(file);
			}
			TempFiles.Clear();

			foreach (var path in TempPaths) {
				try {
					Directory.Delete(path, true);
				} catch { }
			}
			TempPaths.Clear();

			if (GCLevel > 0) System.GC.Collect(GCLevel, GCCollectionMode.Optimized);
		}

		public void ImageCreated() {
			lock (this) CreatedImages++;
		}
	
		public Compiler() {
			NeedsBuilding = true; CheckBuilding = false; SeparateAppDomain = true; NeedsBuildingChecked = false; ChildAppDomain = false;
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
			dest.Parallel = this.Parallel;
			dest.GCLevel = this.GCLevel;
			dest.Serve = this.Serve;
			dest.Cores = this.Cores;
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

	/*	void Init() {
			if (!init) {
				init = true;
				Errors.Message("XamlImageConverter 3.5 by Chris Cavanagh & David Egli");
				Cpus = Parallel ? Environment.ProcessorCount : 1;
				Errors.Message("Using {0} CPU Cores.", Cpus);
			}
		} */

		void LoadDlls() {
			lock (DllLock) {
				var baseDir =  AppDomain.CurrentDomain.BaseDirectory;
				var projDir = ProjectPath;
				if (baseDir.EndsWith("\\")) baseDir = baseDir.Substring(0, baseDir.Length-1);
				if (projDir.EndsWith("\\")) projDir = projDir.Substring(0, projDir.Length-1);
				if (dllsLoaded || string.IsNullOrEmpty(LibraryPath) ||
					(projDir == baseDir && AppDomain.CurrentDomain.RelativeSearchPath.Split(';').Contains(LibraryPath))) return;
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

		/*
		void RawCompile(IEnumerable<string> SourceFiles, string ProjectPath, string LibraryPath) {
	
			foreach (var src in 
			Compile(SourceFiles, RebuildAll, ProjectPath, LibraryPath, true, true, false);
		}
		*/

		void Compile(string filename) {

			SkinPath = ProjectPath;
			bool xaml = false;
			if (filename.Trim()[0] == '#') filename = (string)Context.Session[filename];
			if (filename.Trim()[0] == '<') {
				var res = Parameters.TryGetValue("File", out filename) || Parameters.TryGetValue("Filename", out filename) || Parameters.TryGetValue("Image", out filename);
				filename = MapPath(filename);
				xaml = true;
			}

			var root = new Group();
			root.Filename = filename;
			root.Compiler = this;
			if (!CheckBuilding) {
				root.Errors.Message("XamlImageConverter 3.5 by Chris Cavanagh & David Egli");
				root.Errors.Message("Using {0} CPU Cores.", Cpus);
				root.Errors.Message(Path.GetFileName(filename) + ":");
			}

			if (string.IsNullOrEmpty(ProjectPath)) ProjectPath = Path.GetDirectoryName(filename);
			SkinPath = Path.GetDirectoryName(MapPath(filename));

			List<string> directExtensions = new List<string> { ".xaml", ".psd", ".svg", ".svgz", ".html" };

			XElement config = null;
			DateTime Version = DateTime.Now.ToUniversalTime();
			try {
				if (!RebuildAll) {
					FileInfo info = new FileInfo(filename);
					if (info.Exists) Version = info.LastWriteTimeUtc;
				}
				var ext = Path.GetExtension(filename).ToLower();
				if (xaml) {
					Version = DateTime.MinValue;
					using (var r = new StringReader(filename)) {
						var xdoc = XElement.Load(r, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);
						if (xdoc.Name == xic+"XamlImageConverter" || xdoc.Name == sb+"SkinBuilder") config = xdoc;
						else config = XamlScene.CreateDirect(this, null, xdoc, Parameters);
					}
				} else if (directExtensions.Any(x => ext == x)) {
					config = XamlScene.CreateDirect(this, filename, Parameters);
				} else if (filename == "xic.axd" || filename.EndsWith("\\xic.axd")) {
					config = XamlScene.CreateAxd(this, Parameters);
				} else {
					using (FileLock(filename)) {
						config = XElement.Load(filename, LoadOptions.SetBaseUri | LoadOptions.SetLineInfo | LoadOptions.PreserveWhitespace);
					}
				}
			} catch (FileNotFoundException) {
				root.Errors.Error("Unable to read the configuration file", "1", null);
				return;
			} catch (Exception ex) {
				root.Errors.Error(ex.Message, "21", null);
				return;
			}
			if (config != null) Compile(root, Version, config);
			//if (!CheckBuilding) root.ExitProcess(null);
		}

		public class StepQueue {
			public int Cpus;
			public Compiler Compiler;
			public Group Root;
			public XElement config;
			public bool CheckBuilding;
			public List<Group> scenes;
			public Dictionary<int, List<Step>> steps = new Dictionary<int,List<Step>>();
			public Dictionary<int, List<Step>> todo = new Dictionary<int, List<Step>>();
			public Dictionary<int, Group> roots = new Dictionary<int, Group>();
			public List<Group> total = new List<Group>();
			public DateTime version;
			int running, icpu;
			ManualResetEvent signal = new ManualResetEvent(false);

			public StepQueue(Group root, DateTime version, XElement config, Compiler comp, List<Group> scenes, bool CheckBuilding) {
				Cpus = comp.Cpus;
				running = Cpus;
				Compiler = comp;
				Root = root;
				this.version = version;
				this.config = config;
				this.scenes = scenes;
				this.CheckBuilding = CheckBuilding;
				icpu = Cpus <= 2 ? Cpus - 1 : (new Random().Next(Cpus - 2) + 1);
			}

			int initialized = 0;
			public void Init(int cpu) {
				List<Group> s;
				if (cpu == 0) {
					s = scenes;
					roots[0] = Root;
				} else {
					var root = new Group();
					root.Master = Root;
					roots[cpu] = root;
					s = Compiler.ParseScenes(root, version, config).ToList<Group>();
				}
				//steps[cpu] = s.SelectMany(st => st.Steps()).ToList();
				steps[cpu] = new List<Step>();
				todo[cpu] = new List<Step>();
				foreach (var scene in s) {
					steps[cpu].AddRange(scene.Steps());
				}

				int n;
				lock (this) n = ++initialized;
				
				if (cpu == icpu) {
					var root = new Group();
					root.Master = Root;
					s = Compiler.ParseScenes(root, version, config).OfType<Group>().ToList();
					var seq = s.SelectMany(st => st.Steps()).ToList();
					var mainthread = seq.OfType<Snapshot>()
							.Where(sn => (sn.Scene.Source ?? "").EndsWith(".psd"))
							.ToList<Step>();
					var sequential = seq.OfType<Snapshot>()
							.GroupBy(sn => sn.LocalFilename)
							.Where(g => g.Count() > 1)
							.SelectMany(g => g)
							.ToList<Step>();
					sequential.RemoveAll(sn => mainthread.Contains(sn)); 
					var todosequential = new List<Step>();
					
					n = 0;
					foreach (var st in seq) {
						if (sequential.Contains(st)) todosequential.Add(st);
						if (st is Snapshot) {
							if (!sequential.Contains(st)) {
								for (int c = 0; c < Cpus; c++) {
									if (c == 0 || !mainthread.Contains(st)) todo[c].Add(steps[c][n]);
								}
							}
						} else {
							for (int c = 0; c < Cpus; c++) {
								 if (c == 0 || !mainthread.Contains(st)) todo[c].Add(steps[c][n]);
							}
							if (st is Parameters) todosequential.Add(st); 
						}
						n++;
					}
				
					n = 0;
					foreach (var st in todosequential) todo[cpu].Insert(n++, st);
					if (Cpus > 1) signal.Set();
				} else {
					signal.WaitOne();
				}
			}

			public Group Next(int cpu) {
				lock (this) {
					if (todo[cpu].Count == 0) return null;
					var step = todo[cpu][0];
					int n;
					if (!(step is Parameters) && (n = steps[cpu].IndexOf(step)) >= 0) {
						for (int c = 0; c < Cpus; c++) {
							int ix = todo[c].IndexOf(steps[c][n]);
							if (ix >= 0) todo[c].RemoveAt(ix);
						}
					} else {
						todo[cpu].RemoveAt(0);
					}
					return (Group)step;
				}
			}

			public void Stop(int cpu) {
				lock (this) {
					roots[cpu].Finish();
					if (cpu != 0) {
						List<Process> active;
						lock (roots[cpu]) active = roots[cpu].LocalProcesses.ToList();
						foreach (var p in active) p.WaitForExit();
					}
					running--;
					if (running == 0 && !CheckBuilding) {
						Root.ExitProcess(null);
					}
				}
			}
		}

		void Compile(Group root, DateTime Version, XElement config) {

			if (RebuildAll) Version = DateTime.Now.ToUniversalTime();
			try {
				var scenes = ParseScenes(root, Version, config).ToList();

				var building = CheckNeedsBuilding(scenes);

				NeedsBuilding = NeedsBuilding || building;

				// compile dependencies
				foreach (var scene in scenes) {
					foreach (var dependency in scene.DependsOn) {
						var c = new Compiler();
						this.CopyTo(c);
						c.SourceFiles = new List<string>() { dependency };
						c.SeparateAppDomain = false;
						c.Compile();
					}
				}

				if (!building || CheckBuilding) return;

				LoadDlls();
	
				var steps = new StepQueue(root, Version, config, this, scenes, CheckBuilding); 

				for (int cpu = Cpus-1; cpu >= 0; cpu--) {
					var cpul = cpu;
					ParameterizedThreadStart task = (state) => { // iterate over all steps
						steps.Init(cpul);
						var step = steps.Next(cpul);
						while (step != null) {
							step.Process();
							step = steps.Next(cpul);
						}
						steps.Stop(cpul);
					};

					if (cpul > 0) {
						var thread = new Thread(new ParameterizedThreadStart(task));
						thread.SetApartmentState(ApartmentState.STA);
						if (Culture != null) { thread.CurrentCulture = thread.CurrentUICulture = Culture; }
						thread.Start();
						Threads.Add(thread);
					} else {
						task(null);
					}
				}
			} catch (CompilerException cex) {
				root.Errors.Error(cex.Message, cex.ErrorCode.ToString(), cex.XObject);
			} catch (Exception ex) {
				root.Errors.Warning("An internal error occurred\n\n" + ex.Message + "\n" + ex.StackTrace, "2", null);
			} finally {
			}
		}

		void RawCompile() {
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
			Cpus = Cores ?? (Parallel ? Environment.ProcessorCount : 1);

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
						var setup = new AppDomainSetup();
						setup.ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
						setup.PrivateBinPath = "bin;bin\\Lazy";
						var domain = AppDomain.CreateDomain("XamlImageConverter Compiler", null, setup);
						AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => {
							var file = MapPath("~/bin/Lazy/" + new AssemblyName(args.Name).Name + ".dll");
							if (File.Exists(file)) {
								return Assembly.LoadFrom(file);
							}
							return null;
						};
						
						try 
						{
							Compiler compiler = (Compiler)domain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, "XamlImageConverter.Compiler");
							CopyTo(compiler);
							compiler.SeparateAppDomain = false;
							compiler.ChildAppDomain = true;
							//compiler.STAThread = true;
							compiler.Compile();
							TempFiles.AddRange(compiler.TempFiles);
							TempPaths.AddRange(compiler.TempPaths);
						} catch (Exception ex2) {
						} finally {
							AppDomain.Unload(domain);
						}
					}
				} else {
					CoreCompile();
				}

				Finish();
				if (!ChildAppDomain) {
					if (Serve != null) Serve();
					Cleanup();
				}
				foreach (var logger in Errors.Loggers.OfType<IDisposable>()) logger.Dispose();
			}
		}

		public void Compile(IEnumerable<string> SourceFiles) { this.SourceFiles = SourceFiles.ToList(); Compile(); }
	}

}
