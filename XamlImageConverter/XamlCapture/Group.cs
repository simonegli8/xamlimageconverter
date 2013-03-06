using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Reflection;
using System.Xml.Linq;
using System.IO;

namespace XamlImageConverter {

	public interface Step {
		void Process();
		bool NeedsBuilding { get; }
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
		public Compiler Compiler { get; set; }
		public List<string> DependsOn { get; set; }
		public string Dependencies { set { DependsOn = (value ?? "").Split(';').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList(); } } 
		public string cultures;
		public virtual string CulturesString { get { return Setting(g => g.cultures) ?? ""; } set { cultures = value; } }
		public virtual List<string> Cultures { get { return CulturesString.Split(',', ';').Select(c => c.Trim()).Where(c => !string.IsNullOrEmpty(c)).ToList(); } }
		public Errors Errors { get { return Compiler.Errors; } }
		public List<System.Diagnostics.Process> Processes { get { return Compiler.Processes; } }
		public string Theme { get; set; }
		public string Skin { get; set; }
		public System.Windows.Media.TextRenderingMode? TextRenderingMode { get; set; }
		public System.Windows.Media.TextFormattingMode? TextFormattingMode { get; set; }
		public XElement XElement { get; set; }
		public string OutputPath { get; set; }
		public DateTime Version { get; set; }
		public virtual bool ParseChildren { get { return true; } }
		public string TempPath { get; set; }
		string filename;
		public virtual string Filename { get { return filename; } set { filename = value; localFilename = null; } }
		public static List<string> TempPaths = new List<string>();
		public static List<string> TempFiles = new List<string>();
		static string CurrentTheme = null, CurrentSkin = null;
		//public static Group Root = new Group();
		public ResourceDictionary SkinResources = null;
		public string XamlFile = null;
		public string XamlPath = null;
		public bool IsScene { get; set; }

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
		public Group Root { get { return Scene.Parent; } }
		public Group Parent { get { return (Group)base.Parent; } }

		public string CreateTempPath(string file) {
			TempPath = System.IO.Path.GetTempPath();
			var name = System.IO.Path.GetFileName(file);
			TempPath = System.IO.Path.Combine(TempPath, name + "." + DateTime.Now.Ticks.ToString());
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

		FrameworkElement element = null;
		public FrameworkElement Element {
			get {
				if (element == null) {

					if (Parent == null) { // root element
					} else if (!IsScene) { // load Group or Snapshot
						element = Parent.Element;
					} else { // load Scene
						ApplyStyle();

						if (Source != null) {

							string xaml = null, file = null, directXaml = null, session = null;
							FileInfo info;
							DateTime version;

							if (Source.Trim().StartsWith("<")) directXaml = Source;
							else if (Source.Trim().StartsWith("#")) {
								session = Source;
								directXaml = (string)System.Web.HttpContext.Current.Session["XamlImageConverter.Xaml:" + session];
							} else {
								info = new FileInfo(Compiler.MapPath(Source));
								file = info.FullName;
								version = info.LastWriteTimeUtc;
							}

							bool psd = false;
							if (session == null && (psd = Source.ToLower().EndsWith(".psd")) || Source.ToLower().EndsWith(".swf")) {

								xaml = CreateTempPath(file);

								var apath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
								Type doctype;
								if (psd) {
									var a = Assembly.LoadFrom(Path.Combine(apath, "psd2xaml\\Endogine.Codecs.Photoshop.dll"));
									doctype = a.GetType("Endogine.Codecs.Photoshop.Document");
								} else {
									var a = Assembly.LoadFrom(Path.Combine(apath, "psd2xaml\\Endogine.Codecs.Flash.dll"));
									doctype = a.GetType("Endogine.Codecs.Flash.Document");
								}

								dynamic doc = Activator.CreateInstance(doctype, file);
								doc.SaveXaml(xaml);
								file = xaml;
							}

							if (session == null && Source.ToLower().EndsWith(".svg")) {
								element = SvgConvert.ConvertUtility.LoadSvg(file);
							} else {
								if (directXaml != null) {// file is direct xaml
									using (var sr = new StringReader(directXaml))
									using (var r = System.Xml.XmlReader.Create(sr, new System.Xml.XmlReaderSettings() { CloseInput = true })) {
										element = XamlReader.Load(r) as FrameworkElement;
									}
								} else {
									Scene.XamlFile = file;

									using (var stream = File.OpenRead(file)) {
										if (Source.ToLower().EndsWith(".xaml") || Source.ToLower().EndsWith(".psd") || Source.ToLower().EndsWith(".svg")) {
											ParserContext context;
											if (string.IsNullOrEmpty(XamlElement.BaseUri)) context = new ParserContext { BaseUri = new Uri("file:///" + file) };
											else context = new ParserContext { BaseUri = new Uri(XamlElement.BaseUri) };
											element = XamlReader.Load(stream, context) as FrameworkElement;
										} else {
											throw new CompilerException("Input format not supported.", 20, XElement);
										}
									}
								}
							}
						} else {
							string assemblyName = AssemblyName;
							string typeName = TypeName;
							if (assemblyName != null) assemblyName = Compiler.MapPath(assemblyName);
							Assembly assembly;
							Type type = null;
							if (typeName != null) {
								if (assemblyName != null) {
									try {
										var assemblies = AppDomain.CurrentDomain.GetAssemblies();
										assembly = assemblies.FirstOrDefault(a => a.FullName == assemblyName || (new Uri(a.CodeBase)).LocalPath == assemblyName);
										if (assembly == null) 
											lock (Compiler.DllLock) assembly = Assembly.LoadFrom(assemblyName);
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

		public FrameworkElement ElementClone() {
			/*// save original
			var old = Scene.element;
			// reload xaml
			Scene.element = null;
			Scene.LoadElement();
			// apply parameters
			foreach (var p in Scene.Flatten().TakeWhile(g => g != this).OfType<Parameters>()) p.Process();
 			// save copy
			var clone = Element;
			// clear elements
			element = null;
			foreach (var p in AncestorGroups) p.element = null;
			// restore original
			Scene.element = old;
			// remove clone from logical & visual tree
			//clone.Disconnect();
			*/

			foreach (var p in Ancestors) ClearStyle(p.Element);

			using (var m = new MemoryStream()) {
				XamlWriter.Save(Element, m);
				m.Seek(0, SeekOrigin.Begin);
				return (FrameworkElement)XamlReader.Load(m);
			}

		}

		public void ApplyStyle() {
			// Set OS Theme
			var theme = Setting(g => g.Theme);
			if (CurrentTheme != theme) {
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

			// apply Skin
			var skin = Ancestor(g => g.Skin != null) ?? Scene;
			if (CurrentSkin != skin.Skin) {
				var res = skin.SkinResources;
				if (res == null) {
					try {
						var url = new Uri(skin.Skin);
						Stream source;
						if (url.Scheme == null) source = new FileStream(Compiler.MapPath(skin.Skin), FileMode.Open, FileAccess.Read);
						else if (url.IsFile) source = new FileStream(url.LocalPath, FileMode.Open, FileAccess.Read);
						else {
							var web = new System.Net.WebClient();
							source = web.OpenRead(url);
						}
						using (source) {
							skin.SkinResources = res = (ResourceDictionary)XamlReader.Load(source);
						}
						if (Scene.SkinResources == null) Scene.SkinResources = Application.Current.Resources;
						Application.Current.Resources = res;
					} catch (Exception ex) {
						Errors.Error(string.Format("Error loading Skin {0}.", skin.Skin), "28", skin.XElement ?? XElement);
					}
				}
				CurrentSkin = skin.Skin;
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

#if DEBUG
		public Point RelativeOffset { get { return new Point(Canvas.GetLeft(Window), Canvas.GetTop(Window)); } }
		public IEnumerable<Point> Offsets { get { return Ancestors.SkipLast(1).Select(a => a.RelativeOffset); } }
		public Point TopLeft { get { return new Point(Canvas.GetLeft(Window), Canvas.GetRight(Window)); } }
		public Point Offset { get { return Window.Bounds(Scene.Window).TopLeft; } }
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

		public void Cleanup() {
			if (!string.IsNullOrEmpty(TempPath)) {
				element = null;
				if (!TempPaths.Contains(TempPath)) {
					TempPaths.Add(TempPath);
					TempPath = null;
				}
			}
		}

		public static void Close(List<System.Diagnostics.Process> Processes, Errors errors, IEnumerable<Group> scenes) {
			Snapshot.SaveXps(Processes, errors);
			ImageMagickEncoder.SaveAll(Processes, errors);

			foreach (var scene in scenes) scene.Cleanup();

			foreach (var p in Processes) p.WaitForExit();

			foreach (var file in TempFiles) System.IO.File.Delete(file);

			AppDomain.CurrentDomain.DomainUnload += (sender, args) => {
				foreach (var path in TempPaths) {
					try {
						Directory.Delete(path, true);
					} catch { }
				}
			};
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
							.Select(g => Compiler.MapPath(g.OutputPath))
							.ToList();
						paths.Reverse();

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

						list.Add(Compiler.MapPath(Path.Combine(path, file)));
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
					return Version = new DateTime(LocalFilename.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
						.Max(file => {
							FileInfo info = new FileInfo(file);
							if (info.Exists) return info.LastWriteTimeUtc.Ticks;
							else return DateTime.MinValue.Ticks;
						}));
				} else {
					FileInfo info = new FileInfo(LocalFilename);
					if (info.Exists) return Version = info.LastWriteTimeUtc;
					else return Version = DateTime.MinValue;
				}
			}
		}

		public void EnsureOutputDir() {
			foreach (var file in LocalFilename.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)) {
				var dir = Path.GetDirectoryName(file);
				if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
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

		public void DirectoryCopy(string sourceDirName, string destDirName) {
			DirectoryInfo dir = new DirectoryInfo(sourceDirName);
			if (!dir.Exists) throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
			if (!Directory.Exists(destDirName)) Directory.CreateDirectory(destDirName);
			foreach (FileInfo file in dir.GetFiles()) file.CopyTo(Path.Combine(destDirName, file.Name), true);
			foreach (DirectoryInfo subdir in dir.GetDirectories()) DirectoryCopy(subdir.FullName, Path.Combine(destDirName, subdir.Name));
		}

		public virtual void Process() {
			if (NeedsBuilding) Save();
			else Errors.Note(string.Format("{0} is up to date.", Filename));
		}
	}
}