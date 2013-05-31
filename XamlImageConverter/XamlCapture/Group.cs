﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Documents;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using System.Reflection;
using System.Xml.Linq;
using System.IO;

namespace XamlImageConverter {

	public interface Step {
		void Process();
		bool NeedsBuilding { get; }
		bool MustRunOnMainThread { get; }
		bool MustRunSequential { get; }
	}

	public class Group: Canvas {
		
		public string Source { get; set; }
		public string AssemblyName { get; set; }
		public string TypeName { get; set; }
		public XElement InnerXaml { get; set; }
		public XElement XamlElement { get; set; }
		public XElement SourceElement { get; set; }
		public string ElementName { get; set; }
		public Size PreferredSize { get; set; }
		Compiler compiler;
		public Compiler Compiler { get { return Master.compiler; } set { Master.compiler = value; } }
		public List<string> DependsOn { get; set; }
		public string Dependencies { set { DependsOn = (value ?? "").Split(',', ';').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList(); } } 
		public string cultures;
		public virtual string CulturesString { get { return Setting(g => g.cultures) ?? ""; } set { cultures = value; } }
		public virtual List<string> Cultures { get { return CulturesString.Split(',', ';').Select(c => c.Trim()).Where(c => !string.IsNullOrEmpty(c)).ToList(); } }
		public List<System.Diagnostics.Process> Processes { get { return Compiler.Processes; } }
		//public List<System.Diagnostics.Process> LocalProcesses = new List<System.Diagnostics.Process>();
		Group master;
		public Group Master { get { return Root != this ? Root.Master : (master ?? (master = this)); } set { if (Root != this) Root.Master = value; else master = value; } }
		List<System.Diagnostics.Process> localProcesses = new List<System.Diagnostics.Process>();
		public List<System.Diagnostics.Process> LocalProcesses { get { return Root.localProcesses; } set { Root.localProcesses = value; } }
		public int processCount = 1;
		public int ProcessCount { get { return Master.processCount; } set { Master.processCount = value; } }
		public string Theme { get; set; }
		public string Skin { get; set; }
		public System.Windows.Media.TextRenderingMode? TextRenderingMode { get; set; }
		public System.Windows.Media.TextFormattingMode? TextFormattingMode { get; set; }
		public XElement XElement { get; set; }
		public string OutputPath { get; set; }
		public DateTime Version { get; set; }
		public virtual bool ParseChildren { get { return true; } }
		public int? Layer { get; set; }
		public string TempPath { get; set; }
		string filename;
		public virtual string Filename { get { return filename; } set { filename = value; localFilename = null; } }
		public List<string> TempPaths { get { return Compiler.TempPaths; } }
		public List<string> TempFiles { get { return Compiler.TempFiles; } }
		public string CurrentTheme { get { return Compiler.CurrentTheme; } set { lock (Compiler) Compiler.CurrentTheme = value; } }
		public string CurrentSkin { get { return Compiler.CurrentSkin; } set { lock (Compiler) Compiler.CurrentSkin = value; } }
		//public static Group Root = new Group();
		public ResourceDictionary SkinResources = null;
		public string XamlFile = null;
		public string XamlPath = null;
		public bool IsScene { get; set; }
		public int CreatedImages = 0;
		public DateTime Start = DateTime.Now;
		Errors errors = null;
		public Errors Errors { get { return Master.errors ?? (Master.errors = Compiler.Errors.Clone(Master.Filename)); } set { Master.errors = value; } }
		Dictionary<string, FixedDocument> xpsDocs;
		public Dictionary<string, FixedDocument> XpsDocs { get { return Root.xpsDocs ?? (Root.xpsDocs = new Dictionary<string,FixedDocument>()); } set { Root.xpsDocs = value; } }

		public IDisposable FileLock(string path) { return Compiler.FileLock(path); }

		public void ImageCreated() {
			lock (Master) Master.CreatedImages++;
		}

		public System.Diagnostics.Process NewProcess(string exe, string args = "", string workingdir = null) {
			lock (Processes)
			lock (Master) {
				System.Diagnostics.Process p = null;
				if (exe != null) {
					p = new System.Diagnostics.Process();
					workingdir = workingdir ?? Path.GetDirectoryName(exe);
					var pinfo = p.StartInfo;
					pinfo.FileName = exe;
					pinfo.CreateNoWindow = true;
					pinfo.Arguments = args;
					pinfo.UseShellExecute = false;
					pinfo.RedirectStandardOutput = true;
					pinfo.RedirectStandardError = true;
					pinfo.RedirectStandardInput = true;
					pinfo.WorkingDirectory = workingdir;
					p.EnableRaisingEvents = true;
					Processes.Add(p);
				}
				LocalProcesses.Add(p);
				ProcessCount++;
				return p;
			}
		}

		public void ExitProcess(System.Diagnostics.Process p) {
			lock (Processes)
			lock (Master) {
				if (p != null) {
					Processes.Remove(p);
					LocalProcesses.Remove(p);
				}
				ProcessCount--;
				if (ProcessCount == 0) {
					var time = DateTime.Now - Master.Start;
					Errors.Message("{0} images rendered in {1:G3} seconds.", Master.CreatedImages, time.TotalSeconds);
					Errors.Flush();
				}
			}
		}

		public Group scene = null;
		public Group Scene {
			get {
				if (scene == null) {
					if (IsScene) scene = this;
					else if (Parent == null) scene = null;
					else scene = Parent.Scene;
				}
				return scene;
			}
		}
		public Group Root { get { return Scene != null ? Scene.Parent : this; } }
		public new Group Parent { get { return (Group)base.Parent; } }

		public string CreateTempPath(string file) {
			TempPath = System.IO.Path.GetTempPath();
			var name = System.IO.Path.GetFileName(file);
			var guid = Guid.NewGuid();
			TempPath = System.IO.Path.Combine(TempPath, name + "." + DateTime.Now.Ticks.ToString() + "." + guid.ToString());
			if (!System.IO.Directory.Exists(TempPath)) System.IO.Directory.CreateDirectory(TempPath);
			TempPaths.Add(TempPath);
			Scene.XamlPath = TempPath;
			return System.IO.Path.Combine(TempPath, System.IO.Path.ChangeExtension(name, ".xaml"));
		}
		
		public IEnumerable<Group> Ancestors {
			get {
				var g = this;
				do {
					yield return g;
					g = g.Parent;
				} while (g != null);
			}
		}
		public Group Ancestor(Func<Group, bool> f) { return Ancestors.FirstOrDefault(a => f(a)); }
		public T Setting<T>(Func<Group, T> f) {
			var a = Ancestor(g => f(g) != null);
			return a != null ? f(a) : default(T);
		}

		public class HtmlSource: FrameworkElement {
			public string Source { get; set; }
		}

		FrameworkElement element = null;
		public FrameworkElement Element {
			get {
				if (element == null) {

					if (Parent == null) { // root element
					} else if (!IsScene) { // load Group or Snapshot
						element = Parent.Element;
					} else { // load Scene
						using (ApplyStyleWithLock()) {

							if (Source != null) {

								string xaml = null, file = null, directXaml = null, session = null;
								FileInfo info;
								DateTime version;

								if (Source.Trim().StartsWith("<")) directXaml = Source;
								else if (Source.Trim().StartsWith("#")) {
									session = Source;
									directXaml = (string)Compiler.Context.Session[session];
								} else {
									if (!(Source.StartsWith("http://") || Source.StartsWith("https://"))) {
										info = new FileInfo(Compiler.MapPath(Source));
										file = info.FullName;
										using (FileLock(file)) {
											version = info.LastWriteTimeUtc;
										}
									} else {
										file = Source;
										version = DateTime.MaxValue;
									}
								}

								bool psd = false;
								if (session == null && (psd = Source.ToLower().EndsWith(".psd")) || Source.ToLower().EndsWith(".swf")) {

									xaml = CreateTempPath(file);

									if (Layer == null) {
										try {
											Type doctype;
											var apath = Compiler.BinPath("Lazy\\psd2xaml\\");
											using (FileLock(apath)) {
												if (psd) {
													var a = Assembly.Load(File.ReadAllBytes(apath + "Endogine.dll"));
													a = Assembly.Load(File.ReadAllBytes(apath + "Endogine.Codecs.Photoshop.dll"));
													doctype = a.GetType("Endogine.Codecs.Photoshop.Document");
												} else {
													var a = Assembly.Load(File.ReadAllBytes(apath + "Endogine.dll"));
													a = Assembly.Load(File.ReadAllBytes(apath + "Endogine.Codecs.Flash.dll"));
													doctype = a.GetType("Endogine.Codecs.Flash.Document");
												}
											}

											dynamic doc = Activator.CreateInstance(doctype, file);
											using (FileLock(file)) doc.SaveXaml(xaml);
											file = xaml;
										} catch (Exception ex) {
											if (Source.ToLower().EndsWith(".psd")) Errors.Error("Unable to convert PSD to xaml.", "32", XElement);
											else Errors.Error("Unable to convert SWF to xaml.", "33", XElement);
											Layer = 0;
										}
									}
									if (Layer != null) {
										/* apath = Path.Combine(apath, rpath, "Lazy\\ImageMagick\\");
										var p = NewProcess(apath + "convert.exe", string.Format("{0}[{1}] {2}", file, Layer.Value, xaml + ".png"));
										p.Start();
										p.WaitForExit();
										var xamlns = Compiler.xamlns;
										var xdoc = new XElement(xamlns+"Page",
											new XAttribute(XNamespace.Xmlns+"x", Compiler.xxamlns.NamespaceName),
											new XElement(xamlns+"Canvas",
												new XElement(xamlns+"Rectangle"
										*/
										// ConvertFromPSD(file, xaml, apath);
									}
								}

								if (session == null && Source.ToLower().EndsWith(".svg")) {
									using (FileLock(file)) element = SvgConvert.ConvertUtility.LoadSvg(file);
								} else if (session == null && (Source.ToLower().EndsWith(".html") || Source.ToLower().EndsWith(".htm") || Source.ToLower().StartsWith("http://") || Source.ToLower().StartsWith("https://"))) {
									return new HtmlSource { Source = Source };
								} else {
									if (directXaml != null) {// file is direct xaml
										try {
											var x = XElement.Parse(directXaml, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);
											foreach (var a in x.DescendantsAndSelf()
												.SelectMany(y => y.Attributes().Where(at => at.Name.Namespace == Compiler.xic))) {
												a.Remove();
											}
											using (var r = x.CreateReader()) {
												element = XamlReader.Load(r) as FrameworkElement;
											}
										} catch (Exception ex) {
											throw new CompilerException("Error loading xaml: " + ex.Message, 34, XElement);
										}
									} else {
										Scene.XamlFile = file;

										try {
											XElement xe;
											using (FileLock(file)) {
												xe = XElement.Load(file, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);
											}
											foreach (var a in xe.DescendantsAndSelf()
												.SelectMany(y => y.Attributes().Where(at => at.Name.Namespace == Compiler.xic))) {
												a.Remove();
											}
											using (var r = xe.CreateReader()) {
												if (Source.ToLower().EndsWith(".xaml") || Source.ToLower().EndsWith(".psd") || Source.ToLower().EndsWith(".svg")) {
													//ParserContext context;
													//if (string.IsNullOrEmpty(XamlElement.BaseUri))
													//context = new ParserContext { BaseUri = new Uri("file:///" + file) };
													//else context = new ParserContext { BaseUri = new Uri(XamlElement.BaseUri) };
													element = XamlReader.Load(r) as FrameworkElement;
												} else {
													throw new CompilerException("Input format not supported.", 20, XElement);
												}
											}
										} catch (Exception ex) {
											if (ex is CompilerException) throw ex;
											else throw new CompilerException("Error loading xaml: " + ex.Message, 34, XElement);
										}
									}
								}
							} else {
								string assemblyName = AssemblyName;
								string typeName = TypeName;
								// (assemblyName != null) assemblyName = Compiler.MapPath(assemblyName);
								Assembly assembly;
								Type type = null;
								if (typeName != null) {
									Compiler.LoadDlls();
									if (assemblyName != null) {
										try {
											var assemblies = AppDomain.CurrentDomain.GetAssemblies();
											assembly = assemblies.FirstOrDefault(a => a.FullName == assemblyName || (new Uri(a.CodeBase)).LocalPath == Compiler.MapPath(assemblyName));
											if (assembly == null) {
												lock (Compiler.DllLock) {
													if (assemblyName.Contains(':') || assemblyName.Contains('/') || assemblyName.Contains('\\') || assemblyName.Contains('~')) assembly = Assembly.LoadFrom(Compiler.MapPath(assemblyName));
													else assembly = Assembly.Load(assemblyName);
												}
											}
											if (assembly != null) {
												int i = TypeName.IndexOf(',');
												if (i > -1) typeName = typeName.Substring(0, i);
												type = assembly.GetType(typeName);
											}
										} catch {
											throw new CompilerException(string.Format("Error loading assembly {0}.", assemblyName), 12, XamlElement);
										}
									} else {
										try {
											type = Type.GetType(typeName, false);
											if (type == null) {
												var assemblies = AppDomain.CurrentDomain.GetAssemblies();
												type = assemblies.Reverse().SelectMany(a => a.GetTypes()).FirstOrDefault(t => t.FullName == typeName);
											}
										} catch {
											throw new CompilerException(string.Format("Invalid type name {0}.", typeName), 13, XamlElement);
										}
									}
									if (type != null) {
										try {
											element = Activator.CreateInstance(type) as FrameworkElement;
										} catch (Exception ex) {
											throw new CompilerInnerException(string.Format("Error creating type {0}.", typeName), 14, XamlElement, ex);
										}
									} else {
										throw new CompilerException(string.Format("Type {0} not found.", typeName), 15, XamlElement);
									}
								} else if (InnerXaml != null) {
									element = XamlReader.Load(InnerXaml.CreateReader()) as FrameworkElement;
								} else {
									element = null;
								}
							}

							if (element != null) {
								element.MeasureAndArrange(PreferredSize);
								ApplyStyle();
							}
						}
					}

					//narrow down element to ElementName

					if (ElementName != null) {
						//element = Root.Element.FindName<DependencyObject>(ElementName);
						element = LogicalTreeHelper.FindLogicalNode(Scene.Element, ElementName) as FrameworkElement;
						if (element == null) {
							Errors.Error(string.Format("No element with name \"{0}\" found.", ElementName), "31", this.XElement);
							element = Scene.Element;
						}
					}
					
				}

				return element;
			}
			set { element = value; }
		}

		public void Load() {
			element = Element;
		}

		private void ClearStyle(FrameworkElement e) {
			if (e != null) {
				e.ClearValue(TextOptions.TextHintingModeProperty);
				e.ClearValue(TextOptions.TextRenderingModeProperty);
				e.ClearValue(TextOptions.TextFormattingModeProperty);
			}
		}

		public FrameworkElement VisualClone() {
			// save original
			var old = Scene.Element;
			// reload xaml
			element = null; window = null;
			foreach (var p in Ancestors) { p.element = null; p.window = null; }
			Scene.Load();
			// apply parameters
			foreach (var p in Scene.Flatten().TakeWhile(g => g != this).OfType<Parameters>()) p.Process();
			// clone into a Rectangle's Brush
			var clone = Scene.Element;
			var bounds = Window.Bounds(Scene.Window);

			var brush = new VisualBrush(clone);
			brush.AutoLayoutContent = true;
			brush.Stretch = Stretch.None;
			brush.AlignmentX = AlignmentX.Center;
			brush.AlignmentY = AlignmentY.Center;
			brush.Viewbox = new Rect(bounds.Left/Scene.Window.Width, bounds.Top/Scene.Window.Height, 1, 1);

			var img = new System.Windows.Shapes.Rectangle();
			img.Width = bounds.Width;
			img.Height = bounds.Height;
			img.Fill = brush;

			// clear clone
			element = null; window = null;
			foreach (var p in Ancestors) { p.element = null; p.window = null; }
			// restore original
			Scene.element = old;

			return img;
		}

		public string Xaml(object e) {
			var stringWriter = new StringWriter(new StringBuilder());
			var xmlTextWriter = new System.Xml.XmlTextWriter(stringWriter) { Formatting = System.Xml.Formatting.Indented };
			XamlWriter.Save(e, xmlTextWriter);
			return stringWriter.ToString();
		}

		static List<ThemeLock> Locks = new List<ThemeLock>();
		static object Lock = new object();
		static object lock2 = new object();

		public class ThemeLock : IDisposable {
			public string Theme;
			public ThemeLock() { Theme = ""; }
			public ThemeLock(string theme) {
				System.Threading.Monitor.Enter(this);
				Theme = theme;
				bool otherTheme = false;
				lock (Lock) {
					otherTheme = Locks.Count > 0 && Locks[0].Theme != Theme;
					if (!otherTheme) {
						lock (lock2) Locks.Add(this);
					} else {
						List<ThemeLock> locks;
						lock (lock2) {
							locks = Locks.ToList();
						}
#pragma warning disable
						foreach (var tlock in locks) lock (tlock) ;
#pragma warning restore
					}
				}
			}
			public void Dispose() {
				lock (lock2) {
					Locks.Remove(this);
					System.Threading.Monitor.Exit(this);
				}
			}
		}

		public IDisposable ApplyStyleWithLock() {
			var theme = Setting(g => g.Theme);
			var tlock = new ThemeLock(theme);
			ApplyStyle();
			return tlock;
		}

		public void ApplyStyle() {
			lock (CurrentTheme) {
				// Set OS Theme
				var theme = Setting(g => g.Theme);
				if (CurrentTheme != theme && !string.IsNullOrWhiteSpace(theme)) {
					if (theme == null || theme.Equals("default", StringComparison.OrdinalIgnoreCase)) {
						if (!(string.IsNullOrEmpty(CurrentTheme) && CurrentTheme.Equals("default", StringComparison.OrdinalIgnoreCase))) {
							NorthHorizon.Samples.SystemThemeChange.ThemeHelper.Reset();
							Errors.Message("Resetting OS Theme.");
						}
					} else {
						if (!theme.Equals(CurrentTheme, StringComparison.OrdinalIgnoreCase)) {
							var tokens = theme.ToLower().Split('.');
							var themename = tokens.First();
							var style = tokens.Skip(1).FirstOrDefault() ?? "normalcolor";
							NorthHorizon.Samples.SystemThemeChange.ThemeHelper.SetTheme(themename, style);
							Errors.Message("Setting OS Theme to " + theme);
						}
					}
					CurrentTheme = theme;
				}
			}

			lock (CurrentSkin) {
				// apply Skin
				var skin = Ancestor(g => g.Skin != null) ?? Scene;
				if (CurrentSkin != skin.Skin && !string.IsNullOrWhiteSpace(skin.Skin)) {
					var res = skin.SkinResources;
					if (res == null) {
						try {
							var url = new Uri(skin.Skin);
							Stream source;
							IDisposable flock = null;
							try {
								if (url.Scheme == null) {
									var spath = Compiler.MapPath(skin.Skin);
									flock = FileLock(spath);
									source = new FileStream(spath, FileMode.Open, FileAccess.Read);
								} else if (url.IsFile) {
									flock = FileLock(url.LocalPath);
									source = new FileStream(url.LocalPath, FileMode.Open, FileAccess.Read);
								} else {
									var web = new System.Net.WebClient();
									source = web.OpenRead(url);
								}
								using (source) {
									skin.SkinResources = res = (ResourceDictionary)XamlReader.Load(source);
								}
							} catch {
							} finally {
								if (flock != null) flock.Dispose();
							}
							if (Scene.SkinResources == null) Scene.SkinResources = Application.Current.Resources;
							Application.Current.Resources = res;
						} catch (Exception ex) {
							Errors.Error(string.Format("Error loading Skin {0}.", skin.Skin), "28", skin.XElement ?? XElement);
						}
					}
					CurrentSkin = skin.Skin;
				}
			}
			if (element != null) {
				TextOptions.SetTextHintingMode(Element, TextHintingMode.Fixed);
				var rendering = Setting(g => g.TextRenderingMode);
				if (rendering.HasValue) TextOptions.SetTextRenderingMode(Element, rendering.Value);
				var formatting = Setting(g => g.TextFormattingMode);
				if (formatting.HasValue) TextOptions.SetTextFormattingMode(Element, formatting.Value);
			}
		}

		Group window = null;
		public Group Window {
			get {
				if (Parent == null) return null;

				if (window == null) {
					window = new Group();
					if (Parent != null && Parent.Window != null && !Parent.Window.Children.Contains(window)) Parent.Window.Children.Add(window);

					var top = Canvas.GetTop(this);
					var bottom = Canvas.GetBottom(this);
					var left = Canvas.GetLeft(this);
					var right = Canvas.GetRight(this);
					var hasSize = !(double.IsNaN(top) && double.IsNaN(bottom) && double.IsNaN(right) && double.IsNaN(left) && double.IsNaN(Width) && double.IsNaN(Height));

					Rect bounds;
					if (IsScene) bounds = Element.Bounds(Scene.Element);
					else bounds = Element.Bounds(Parent.Element);
					window.Width = bounds.Width;
					window.Height = bounds.Height;

					/*if (ElementName != null && Parent != null) {
						var parentBounds = Parent.Window.Bounds(Scene.Window);
						Canvas.SetLeft(window, bounds.X - parentBounds.X);
						Canvas.SetTop(window, bounds.Y - parentBounds.Y);
					} else { */
						Canvas.SetLeft(window, bounds.X);
						Canvas.SetTop(window, bounds.Y);
					//}
					window.MeasureAndArrange(new Size(window.Width, window.Height));

					if (hasSize) {
						var inner = new Group();
						Canvas.SetTop(inner, top);
						Canvas.SetBottom(inner, bottom);
						Canvas.SetLeft(inner, left);
						Canvas.SetRight(inner, right);
						inner.Width = double.IsNaN(Width) ? window.Width : Width;
						inner.Height = double.IsNaN(Height) ? window.Height : Height;
						window.Children.Add(inner);
						window = inner;
						inner.MeasureAndArrange(new Size(window.Width, window.Height));
					}
				}

				return window;
			}
		}

		public Group ElementGroup {
			get {
				if (Parent == null) return null;
				if (IsScene || !string.IsNullOrEmpty(ElementName)) return this;
				return Parent.ElementGroup;
			}
		}

		public Point RelativeOffset { get { return new Point(Canvas.GetLeft(Window), Canvas.GetTop(Window)); } }
		public Point TopLeft { get { return RelativeOffset; } }
		public Point Offset { get { return Window.Bounds(Scene.Window).TopLeft; } }

#if DEBUG
		public IEnumerable<Point> Offsets { get { return Ancestors.SkipLast(1).Select(a => a.RelativeOffset); } }
#endif

		public Transform Transform {
			get {
				if (Window.Parent == null) return new TranslateTransform(0, 0);
				var offset = Window.Bounds(Scene.Window).TopLeft;
				return new TranslateTransform(-offset.X, -offset.Y);
			}
		} 

		//public FileInfo FileInfo { get; set; }

		/// <summary>
		/// Return all child steps
		/// </summary>
		/// <returns>Returns all child steps</returns>
		public virtual IEnumerable<Step> Flatten() {
			var steps = Children.OfType<Group>()
				.SelectMany(ch => ch.Flatten())
				.OfType<Step>();
			if (this is Step) return steps.Prepend((Step)this);
			return steps;
		}

		public IEnumerable<Step> Steps() { return Flatten(); }

		public virtual void Cleanup() {
			foreach (var child in Children.OfType<Group>()) child.Cleanup(); 
		}

		public void SaveXps() {
			foreach (var key in XpsDocs.Keys.ToList()) {
				var doc = XpsDocs[key];

				var dir = Path.GetDirectoryName(key);
				if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
				if (File.Exists(key)) File.Delete(key);

				using (FileLock(key))
				using (var xpsd = new XpsDocument(key, FileAccess.ReadWrite)) {
					// var fds = xpsd.AddFixedDocumentSequence();
					// var w = fds.AddFixedDocument();
					var xw = XpsDocument.CreateXpsDocumentWriter(xpsd);
					xw.Write(doc);
					//xpsd.Close();
				}

				XpsDocs.Remove(key);

				string final;

				if (key.EndsWith("._temp.xps")) { // convert xps to final format
					TempFiles.Add(key);
					var filename = key.Substring(0, key.Length - "._temp.xps".Length);
					var ext = Path.GetExtension(filename);
					string device, exe;

					//TODO ps2write doesn't work!? Use ghostscript to convert pdf to ps.

					if (ext == ".pdf") {
						device = "pdfwrite";
						exe = "gxps-9.05-win32.exe";
					} else {
						device = "ps2write";
						exe = "gxps-9.07-win32.exe";
					}
					exe = Compiler.BinPath("Lazy\\gxps\\" + exe);
					//if (!File.Exists(exe)) exe = Path.Combine(path, @"\gxps.exe");
					var args = string.Format("-sDEVICE={0} -dNOPAUSE \"-sOutputFile={1}\" \"{2}\"", device, filename, key);
					var process = NewProcess(exe, args, Path.GetDirectoryName(filename));
					//var keylock = FileLock(key);
					//var filelock = FileLock(filename);
					process.Exited += (sender, arg) => {
						//filelock.Dispose();
						//keylock.Dispose();
						final = filename;
						var info = new FileInfo(final);
						Errors.Message("Created {0} ({1} pages, {2:f2} MB)", info.Name, doc.Pages.Count, (double)info.Length / (1024 * 1024));
						ImageCreated();
						ExitProcess(process);
					};
					process.Start();
				} else {
					final = key;
					using (FileLock(final)) {
						var info = new FileInfo(final);
						Errors.Message("Created {0} ({1} pages, {2:f2} MB)", info.Name, doc.Pages.Count, (double)info.Length / (1024 * 1024));
					}
					ImageCreated();
				}
			}
		}

		public virtual void Finish() {
			SaveXps();
			//ImageMagickEncoder.SaveAll(Processes, compiler.Errors);

			Cleanup();
		}

		string localFilename = null;
		public string LocalFilename {
			get {
				if (localFilename != null) return localFilename;

				var list = new List<string>();
				foreach (var file in Filename.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)) {

					if (file.StartsWith("~")) list.Add(Compiler.MapPath(file));
					else if (file.Contains(':')) list.Add(file);
					else {

						var paths = Ancestors.Where(g => !string.IsNullOrEmpty(g.OutputPath))
							.Select(g => g.OutputPath.Replace('/', '\\'))
							.ToList();

						string path = string.Empty;
						while (paths.Count > 0) {
							if (paths[0].StartsWith("~")) {
								path = Path.Combine(Compiler.MapPath(paths[0]), path);
								break;
							}
							path = Path.Combine(paths[0], path);
							if (paths[0].Contains(':')) break;
							paths.RemoveAt(0);
						}

						list.Add(new FileInfo(Compiler.MapPath(Path.Combine(path, file))).FullName);
					}
				}	
				var sb = new StringBuilder();
				if (list.Count > 0) sb.Append(list[0]);
				int i = 1;
				while (i < list.Count) {
					sb.Append(';');
					sb.Append(list[i++]);
				}
				return sb.ToString();
			}
		}

		public DateTime OutputVersion {
			get {
				if (LocalFilename.Contains(',') || LocalFilename.Contains(';')) {
					var locks = LocalFilename.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
						.Select(file => FileLock(file));

					Version = new DateTime(LocalFilename.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
						.Max(file => {
							FileInfo info = new FileInfo(file);
							if (info.Exists) return info.LastWriteTimeUtc.Ticks;
							else return DateTime.MinValue.Ticks;
						}));

					foreach (var file in locks) file.Dispose();

					return Version;
				} else {
					FileInfo info = new FileInfo(LocalFilename);
					using (FileLock(LocalFilename)) {
						if (info.Exists) return Version = info.LastWriteTimeUtc;
						else return Version = DateTime.MinValue;
					}
				}
			}
		}

		public void EnsureOutputDir() {
			foreach (var file in LocalFilename.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)) {
				var dir = Path.GetDirectoryName(file);
				using (FileLock(dir)) {
					if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
				}
			}
		}

		public virtual bool NeedsBuilding {
			get {
				if (OutputVersion >= Scene.Version) return false;
				EnsureOutputDir();
				return true;
			}
		}

		public virtual void Save() { }

		public virtual bool MustRunOnMainThread { get { return false; } }
		public virtual bool MustRunSequential { get { return false; } }

		public void DirectoryCopy(string sourceDirName, string destDirName) {
			DirectoryInfo dir = new DirectoryInfo(sourceDirName);
			if (!dir.Exists) throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
			if (!Directory.Exists(destDirName)) Directory.CreateDirectory(destDirName);
			foreach (FileInfo file in dir.GetFiles()) file.CopyTo(Path.Combine(destDirName, file.Name), true);
			foreach (DirectoryInfo subdir in dir.GetDirectories()) DirectoryCopy(subdir.FullName, Path.Combine(destDirName, subdir.Name));
		}

		public virtual void Process() {
			try {
				if (NeedsBuilding) Save();
				else Errors.Note(string.Format("{0} is up to date.", Filename));
			} catch (CompilerException cex) {
				//Errors.Error(cex.Message, cex.ErrorCode.ToString(), cex.XObject);
				Compiler.HandleException(cex, Errors);
			} catch (Exception ex) {
				Errors.Warning("An internal error occurred\n\n" + ex.Message + "\n" + ex.StackTrace, "2", XElement);
			}
		}
	}
}