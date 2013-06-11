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
		public CompilerException(string msg, int code, XObject xobj, Exception inner) : base(msg, inner) { ErrorCode = code; XObject = xobj; }
	}

	public interface ICompiler {
		List<string> SourceFiles { get; set; }
		string ProjectPath { get; set; }
		string LibraryPath { get; set; }
		bool RebuildAll { get; set; }
		bool UseService { get; set; }
		bool SeparateAppDomain { get; set; }
		bool Parallel { get; set; }
		int? Cores { get; set; }
		int GCLevel { get; set; }
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
		int gclevel = 1;
		public int GCLevel { get { return SeparateAppDomain ? 0 : gclevel; } set { gclevel = value; } }
		public int? Cores { get; set; }
		public Action Serve { get; set; }
		public CultureInfo Culture { get; set; }
		public Dictionary<string, string> Parameters { get; set; }
		static int id = 0;
		public List<Process> Processes { get; set; }
		public System.Web.HttpContext Context { get; set; }  
		public int? hash { get; set; }
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
		int CreatedImages = 0;
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

			public FileLocks(string path) {
				this.path = path;
				lock (Locks) {
					if (!Locks.ContainsKey(path)) Locks.Add(path, new object());
				}
				Monitor.Enter(Locks[path]);
			}

			public void Dispose() {
				Monitor.Exit(Locks[path]);
			}
		}

		public IDisposable FileLock(string path) {
			return new FileLocks(path);
		}

		public void Cleanup() {
			foreach (var file in TempFiles) {
				try {
					System.IO.File.Delete(file);
				} catch { }
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
	
		public void InitDomain() {
			AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => {
				var aname = new AssemblyName(args.Name);
				return AppDomain.CurrentDomain.GetAssemblies()
					.FirstOrDefault(a => a.FullName.StartsWith(aname.FullName));
			};
		}

		public void HandleException(CompilerException ex, Errors errors) {
			var msg = new StringBuilder();
			msg.AppendLine("Internal error occurred:");
			msg.AppendLine(ex.Message);
			msg.AppendLine(ex.StackTrace);
			var iex = ex.InnerException;
			while (iex != null) {
				msg.AppendLine();
				msg.AppendLine("Inner Exception:");
				msg.Append(iex.Message);
				msg.Append(iex.StackTrace);
				iex = iex.InnerException;
			}
			errors.Error(msg.ToString(), ex.ErrorCode.ToString(), ex.XObject);
		}

		public Compiler() {
			NeedsBuilding = true; CheckBuilding = false; SeparateAppDomain = true; NeedsBuildingChecked = false; ChildAppDomain = false; Cores = null; Parallel = true;
			RebuildAll = false; UseService = false;
			SourceFiles = new List<string>();
			Parameters = new Dictionary<string, string>();
			Processes = new List<Process>();
			hash = null;
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
			dest.Serve = null;
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
			if (path.StartsWith("http://") || path.StartsWith("https://")) return path;
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

		public string BinPath(string path) {
			var bpath = AppDomain.CurrentDomain.BaseDirectory;
			var dpath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
			var paths = (";" + AppDomain.CurrentDomain.RelativeSearchPath)
				.Split(';')
				.Select(p => Path.Combine(bpath, p))
				.ToList();
			paths.Insert(0, dpath);

			foreach (var p in paths.ToArray()) paths.Insert(0, Path.Combine(p, "Lazy"));
			return paths.Select(p => Path.Combine(p, path))
				.FirstOrDefault(p => File.Exists(p) || Directory.Exists(p));
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
				Errors.Message("XamlImageConverter 3.10 by Chris Cavanagh & David Egli");
				Cpus = Parallel ? Environment.ProcessorCount : 1;
				Errors.Message("Using {0} CPU Cores.", Cpus);
			}
		} */

		public void LoadDlls() {
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
							var aname = new AssemblyName();
							aname.CodeBase = new Uri(file).ToString();
							assembly = Assembly.Load(aname);

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
			string dxaml = null;
			if (filename.Trim()[0] == '#') filename = (string)Context.Session[filename];
			if (filename.Trim()[0] == '<') {
				dxaml = filename;
				var res = Parameters.TryGetValue("File", out filename) || Parameters.TryGetValue("Filename", out filename) || Parameters.TryGetValue("Image", out filename);
				if (!res) filename = Path.Combine(SkinPath, "direct.xic.xaml");
				xaml = true;
			}
			filename = MapPath(filename);

			var root = new Group();
			root.Filename = filename;
			root.Compiler = this;
			if (!CheckBuilding) {
				root.Errors.Status("XamlImageConverter 3.10 by Chris Cavanagh & David Egli");
				root.Errors.Status("{0:G}, using {1} CPU cores.", DateTime.Now, Cpus);
				root.Errors.Status(Path.GetFileName(filename) + ":");
			}

			if (string.IsNullOrEmpty(ProjectPath)) ProjectPath = Path.GetDirectoryName(filename);
			SkinPath = Path.GetDirectoryName(filename);

			List<string> directExtensions = new List<string> { ".xaml", ".psd", ".svg", ".svgz", ".html" };

			XElement config = null;
			DateTime Version = DateTime.MaxValue;
			try {
				if (!RebuildAll) {
					FileInfo info = new FileInfo(filename);
					if (info.Exists) Version = info.LastWriteTimeUtc;
				}
				var ext = Path.GetExtension(filename).ToLower();
				if (xaml) {
					Version = DateTime.MinValue;
					using (var r = new StringReader(dxaml)) {
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
				root.Errors.Error("Unable to read the configuration file", "1");
				return;
			} catch (Exception ex) {
				root.Errors.Error(ex.Message, "21", null, ex);
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
			public List<Group> scenes = new List<Group>();
			public List<List<Step>> steps = new List<List<Step>>();
			public List<Queue<Step>> todo = new List<Queue<Step>>();
			public HashSet<int> done = new HashSet<int>();
			public List<Group> roots = new List<Group>();
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
				for (int i = 0; i < Cpus; i++) {
					steps.Add(new List<Step>());
					todo.Add(new Queue<Step>());
					roots.Add(null);
				}
				roots.Add(null);
				Contract.Assert(icpu < Cpus);
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
				steps[cpu] = s.SelectMany(st => st.Steps()).ToList();
				todo[cpu] = new Queue<Step>();

				var mysteps = steps[cpu];
				int n;
				lock (this) n = ++initialized;
				
				if (cpu == icpu && cpu > 0) {
					var root = new Group();
					root.Master = Root;
					roots[Cpus] = root;
					s = Compiler.ParseScenes(root, version, config).OfType<Group>().ToList();
					mysteps = s.SelectMany(st => st.Steps()).ToList();
					running++;
				}
				var first = new Queue<Step>(); // steps that should be processed first
				var later = new Queue<Step>(); // steps that should be processed second
				var hasFirst = false;

				for (int i = 0; i < mysteps.Count; i++) { // split steps into first & later queues
					var st = mysteps[i];
					var isSeq = st.MustRunSequential && Cpus > 1; // do not process sequential steps special with only 1 cpu
					if (isSeq) hasFirst = true;
					if (st is Parameters || isSeq) first.Enqueue(st);
					if (!isSeq && (cpu == 0 || !st.MustRunOnMainThread)) later.Enqueue(steps[cpu][i]);
				}

				var ct = todo[cpu]; // merge first & later queues into todo[cpu]
				if (hasFirst && cpu == icpu && Cpus > 1) {
					while (first.Count > 0) ct.Enqueue(first.Dequeue());
				}
				while (later.Count > 0) ct.Enqueue(later.Dequeue());
			}

			public Step Next(int cpu) {
				lock (this) {
					int n = -1;
					Step step = null;
					do {
						if (todo[cpu].Count == 0) return null;
						done.Add(n);
						step = todo[cpu].Dequeue();
						n = steps[cpu].IndexOf(step);
					} while (!(step is Parameters) && n >= 0 && done.Contains(n));
					done.Add(n);
					return step;
				}
			}

			public void Stop(int cpu) {
				lock (this) {
					if (cpu == icpu && Cpus > 1) Stop(Cpus);
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

				if (!building || CheckBuilding) {
					if (!CheckBuilding) root.Errors.Clear();
					return;
				}

				LoadDlls();
	
				var steps = new StepQueue(root, Version, config, this, scenes, CheckBuilding); 

				for (int cpu = Cpus-1; cpu >= 0; cpu--) {
					var cpul = cpu;
					ParameterizedThreadStart task = (state) => { // iterate over all steps
						try {
							steps.Init(cpul);
							try {
								var step = steps.Next(cpul);
								while (step != null) {
									try {
										step.Process();
										step = steps.Next(cpul);
									} catch (CompilerException cex) {
										//root.Errors.Error(cex.Message, cex.ErrorCode.ToString(), cex.XObject);
										HandleException(cex, root.Errors);
									} catch (Exception ex) {
										root.Errors.Warning("An internal error occurred\n\n" + ex.Message + "\n" + ex.StackTrace, "2", null);
									}
								}
							} catch (CompilerException cex) {
								//root.Errors.Error(cex.Message, cex.ErrorCode.ToString(), cex.XObject);
								HandleException(cex, root.Errors);
							} catch (Exception ex) {
								root.Errors.Warning("An internal error occurred\n\n" + ex.Message + "\n" + ex.StackTrace, "2", null);
							} finally {
								steps.Stop(cpul);
							}
						} catch (CompilerException cex) {
							//root.Errors.Error(cex.Message, cex.ErrorCode.ToString(), cex.XObject);
							HandleException(cex, root.Errors);
						} catch (Exception ex) {
							root.Errors.Warning("An internal error occurred\n\n" + ex.Message + "\n" + ex.StackTrace, "2", null);
						} finally {
						}
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
				//root.Errors.Error(cex.Message, cex.ErrorCode.ToString(), cex.XObject);
				HandleException(cex, root.Errors);
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
						var current = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
						var setup = new AppDomainSetup();
						setup.ApplicationBase = ProjectPath;
						setup.PrivateBinPath = MapPath("~/bin") + ";" + MapPath("~/bin/Lazy") + ";" + MapPath("~/bin/Debug") + ";" + MapPath("~/bin/Release") + ";" + MapPath("~/");
						setup.ShadowCopyDirectories = setup.PrivateBinPath + ";" + current;
						setup.ShadowCopyFiles = "true";
						var domain = AppDomain.CreateDomain("XamlImageConverter Compiler", null, setup);
						var aname = Assembly.GetExecutingAssembly().GetName();

						var source = setup.PrivateBinPath.Split(';')
							.Select(p => Path.Combine(p, "XamlImageConverter.dll"))
							.FirstOrDefault(p => File.Exists(p));

						try 
						{

							Compiler compiler = null;
							object proxy;
							if (source == null)
								proxy = domain.CreateInstanceFromAndUnwrap(aname.CodeBase, "XamlImageConverter.Compiler");
							else
								proxy = domain.CreateInstanceAndUnwrap(aname.FullName, "XamlImageConverter.Compiler");
							compiler = (Compiler)proxy;
							
							compiler.InitDomain();
							CopyTo(compiler);
							compiler.SeparateAppDomain = false;
							compiler.ChildAppDomain = true;
							//compiler.STAThread = true;
							compiler.Compile();
							hash = compiler.hash;
						} catch (Exception ex2) {
							Errors.Error("Internal Error, unable to create child AppDomain", "33", null, ex2);

						} finally {
							AppDomain.Unload(domain);
						}
					}
				} else {
					CoreCompile();
				}

				Finish();
				if (Serve != null) Serve();
				Cleanup();

				foreach (var logger in Errors.Loggers.OfType<IDisposable>()) logger.Dispose();
			}
		}

		public void Compile(IEnumerable<string> SourceFiles) { this.SourceFiles = SourceFiles.ToList(); Compile(); }
	}

}
