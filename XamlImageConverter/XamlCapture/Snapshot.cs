using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Text;
using System.Threading;
using System.Globalization;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;

namespace XamlImageConverter {
	/// <summary>
	/// Snapshot definition
	/// </summary>
	public class Snapshot: Group, Step {

		public string StoryboardName { get; set; }

		Storyboard storyboard = null;
		public Storyboard Storyboard {
			get {
				if (StoryboardName != null && storyboard == null) storyboard = (Scene.Element as FrameworkElement).FindResource<Storyboard>(StoryboardName);
				return storyboard;
			}
		}
		public int? Frames { get; set; }
		public bool Filmstrip { get; set; }
		public double? Dpi { get; set; }
		public int? Quality { get; set; }
		public string Page { get; set; }
		public bool FitToPage { get; set; }
		public int Loop { get; set; }
		public double Pause { get; set; }

		public IEnumerable<BitmapSource> Bitmaps;

		/// <summary>
		/// Save a snapshot of a WPF element
		/// </summary>
		/// <param name="element">WPF element</param>
		/// <returns>The current <see cref="Snapshot"/> with a populated Bitmaps collection</returns>
		public override void Save() {
			if (Cultures.Count > 0) {
				foreach (var culture in Cultures) {
					Save(culture);
				}
			} else {
				Save(null);
			}
		}

		static Dictionary<string, FixedDocument> XpsDocs = new Dictionary<string, FixedDocument>();

		double ParseLength(string token) {
			double res = 0;
			if (token.EndsWith("mm") && double.TryParse(token.Substring(0, token.Length-2), out res)) return res*3.779528;
			if (token.EndsWith("cm") && double.TryParse(token.Substring(0, token.Length-2), out res)) return res*37.79528;
			if (token.EndsWith("in") && double.TryParse(token.Substring(0, token.Length-2), out res)) return res*96;
			if (token.EndsWith("pt") && double.TryParse(token.Substring(0, token.Length-2), out res)) return res*1.33333333;
			double.TryParse(token, out res);
			return res;
		}

		public Size GetSize() {
			Size size = new Size(96 * 8.3, 96 * 11.7); // A4;
			if (!string.IsNullOrEmpty(Page)) {
				var page = Page.ToLower();
				if (page.Contains("x") || page.Contains(",")) {
					var tokens = page.Split('x', ',');
					for (int i = 0; i < tokens.Length; i++) tokens[i] = tokens[i].Trim();
					if (tokens.Length == 2) {
						size = new Size(ParseLength(tokens[0]), ParseLength(tokens[1]));
					}
					return size;
				}
				switch (page) {

				case "a0": return new Size(96 * 33.1, 96 * 46.8);
				case "a0l": return new Size(96 * 46.8, 96 * 33.1);
				case "a1": return new Size(96 * 23.4, 96 * 33.1);
				case "a1l": return new Size(96 * 33.1, 96 * 23.4);
				case "a2": return new Size(96 * 16.5, 96 * 23.4);
				case "a2l": return new Size(96 * 23.4, 96 * 16.5);
				case "a3": return new Size(96 * 11.7, 96 * 16.5);
				case "a3l": return new Size(96 * 16.5, 96 * 11.7);
				case "a4": return new Size(96 * 8.3, 96 * 11.7);
				case "a4l": return new Size(96 * 11.7, 96 * 8.3);
				case "a5": return new Size(96 * 5.8, 96 * 8.3);
				case "a5l": return new Size(96 * 8.3, 96 * 5.8);
				case "a6": return new Size(96 * 4.1, 96 * 5.8);
				case "a6l": return new Size(96 * 5.8, 96 * 4.1);
				case "a7": return new Size(96 * 2.9, 96 * 4.1);
				case "a7l": return new Size(96 * 4.1, 96 * 2.9);
				case "a8": return new Size(96 * 2.0, 96 * 2.9);
				case "a8l": return new Size(96 * 2.9, 96 * 2.0);
				case "a9": return new Size(96 * 1.5, 96 * 2.0);
				case "a9l": return new Size(96 * 2.0, 96 * 1.5);
				case "a10": return new Size(96 * 1.0, 96 * 1.5);
				case "a10l": return new Size(96 * 1.5, 96 * 1.0);
				case "letter": return new Size(96 * 8.5, 96 * 11);
				case "letterl": return new Size(96 * 11, 96 * 8.5);
				case "legal": return new Size(96 * 14, 96 * 8.5);
				case "legall": return new Size(96 * 8.5, 96 * 14);
				case "junior": return new Size(96 * 5, 96 * 8);
				case "juniorl": return new Size(96 * 8, 96 * 5);
				case "ledger": return new Size(96 * 11, 96 * 17);
				case "tabloid": return new Size(96 * 17, 96 * 11);
				default:
					FrameworkElement e;
					if (string.IsNullOrEmpty(ElementName)) e = Scene.Element as FrameworkElement;
					else e = Element as FrameworkElement;
					return e.DesiredSize;
				}
			}
			return size;
		}

		private void SaveXpsPage(string filename) {
	
			if (!XpsDocs.ContainsKey(filename)) {
				var d = new FixedDocument();
				d.DocumentPaginator.PageSize = GetSize();
				XpsDocs.Add(filename, d);
			}
			var doc = XpsDocs[filename];
			var size = doc.DocumentPaginator.PageSize;
			var cont = new PageContent();
			var pg = new FixedPage();
			var canvas = new Canvas();

			var clone = ElementClone();

			clone.MeasureAndArrange(new Size(clone.ActualWidth, clone.ActualHeight));

			canvas.Children.Add(clone);
			canvas.Width = size.Width;
			canvas.Height = size.Height;
	
			pg.Width = size.Width;
			pg.Height = size.Height;
			pg.Background = Brushes.White;
			pg.Children.Add(canvas);
			FixedPage.SetLeft(canvas, 0);
			FixedPage.SetRight(canvas, 0);

			((IAddChild)cont).AddChild(pg);

			cont.Measure(size);
			cont.Arrange(new Rect(new Point(), size));
			cont.UpdateLayout();

			var left = Canvas.GetLeft(this);
			var right = Canvas.GetRight(this);
			var top = Canvas.GetTop(this);
			var bottom = Canvas.GetBottom(this);
			var width = Width;
			var height = Height;

			if (!double.IsNaN(left) && !double.IsNaN(right)) { width = right - left; right = double.NaN; }
			if (!double.IsNaN(top) && !double.IsNaN(bottom)) { height = bottom - top; bottom = double.NaN; }
			if (!double.IsNaN(width) && !double.IsNaN(right)) { left = clone.DesiredSize.Width - right - width; right = double.NaN; }
			if (!double.IsNaN(height) && !double.IsNaN(bottom)) { top = clone.DesiredSize.Height - bottom - height; bottom = double.NaN; }
			if (double.IsNaN(left)) left = 0;
			if (double.IsNaN(top)) top = 0;
			if (double.IsNaN(width)) width = clone.ActualWidth;
			if (double.IsNaN(height)) height = clone.ActualHeight;
			Canvas.SetTop(clone, (size.Height - height) / 2);
			Canvas.SetLeft(clone, (size.Width - width) / 2);
			var clip = new RectangleGeometry();
			clip.Rect = new Rect(left, top, width, height);
			clone.Clip = clip;

			double zoom = 1;
			if (FitToPage) zoom = Math.Min(size.Width / width, size.Height / height);
			if (zoom != 1) {
				canvas.RenderTransform = new ScaleTransform(zoom, zoom);
				canvas.RenderTransformOrigin = new Point(size.Width / 2, size.Height / 2);
			}

			cont.Measure(size);
			cont.Arrange(new Rect(new Point(), size));
			cont.UpdateLayout();

			doc.Pages.Add(cont);
		}

		string XpsTempFile {	get { return LocalFilename + "._temp.xps"; } }

		public virtual Snapshot Save(string Culture) {
			var oldculture = Thread.CurrentThread.CurrentCulture;
			var olduiculture = Thread.CurrentThread.CurrentUICulture;
			var ext = Path.GetExtension(Filename);
			string filename = LocalFilename;

			if (!string.IsNullOrEmpty(Culture)) {
				filename = Path.GetFileNameWithoutExtension(filename) + "." + Culture + ext;
				var info = new CultureInfo(Culture);
				Thread.CurrentThread.CurrentCulture = info;
				Thread.CurrentThread.CurrentUICulture = info;
			}

			ext = ext.ToLower();
			if (ext == ".eps" || ext == ".ps" || ext == ".pdf") SaveXpsPage(filename + "._temp.xps");
			else if (ext == ".xps") SaveXpsPage(filename);
			else if (ext == ".xaml") {
				var element = Element ?? Scene;
				var xamlsave = false;
				if (Element == null) {
					if (Scene.XamlPath != null) {
						DirectoryCopy(Scene.XamlPath, Path.GetDirectoryName(filename));
						File.Move(Path.Combine(Path.GetDirectoryName(filename), Path.GetFileName(Scene.XamlFile)), filename);
						File.WriteAllText(LocalFilename, File.ReadAllText(filename).Replace(Scene.XamlPath + "\\", ""));
					} else if (Scene.XamlFile != null) File.Copy(Scene.XamlFile, filename);
					else if (Scene.Source.ToLower().EndsWith(".svg") || Scene.Source.ToLower().EndsWith(".svgz")) {
						SvgConvert.ConvertUtility.ConvertSvg(Compiler.MapPath(Scene.Source), filename);
					} else xamlsave = true;
				} else {
					xamlsave = true;
				}
				if (xamlsave) {
					var settings = new System.Xml.XmlWriterSettings();
					settings.CheckCharacters = true;
					settings.CloseOutput = true;
					settings.Encoding = Encoding.UTF8;
					settings.Indent = true;
					settings.IndentChars = "  ";
					using (var w = System.Xml.XmlWriter.Create(filename, settings)) {
						try {
							XamlWriter.Save(element, w);
						} catch { }
					}
				}
				Errors.Message("Created {0} ({1} MB RAM used)", Path.GetFileName(Filename), System.Environment.WorkingSet / (1024 * 1024));
			} else {
				Bitmaps = Capture.GetBitmaps(this).ToList();
				var encoder = CreateEncoder(filename, Quality, Dpi);

				if (Filmstrip) encoder.Frames.Add(BitmapFrame.Create(MakeFilmstrip(Bitmaps, Dpi)));
				else foreach (var b in Bitmaps) encoder.Frames.Add(BitmapFrame.Create(b));

				encoder.Save(filename);

				if (Loop != 1 && ext == ".gif") {
					string file = Path.GetFileName(filename);
					var path = Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath);
					var exe = Path.Combine(path, "ImageMagick\\convert.exe");
					var args = " -loop " + Loop;
					if (Pause != 0) args += " -pause " + Pause;
					args += " " + file + " " + file;
					var pinfo = new System.Diagnostics.ProcessStartInfo(exe);
					pinfo.CreateNoWindow = true;
					pinfo.Arguments = args;
					pinfo.UseShellExecute = false;
					pinfo.RedirectStandardOutput = true;
					pinfo.RedirectStandardError = true;
					pinfo.RedirectStandardInput = true;
					pinfo.WorkingDirectory = Path.GetDirectoryName(filename);
					var process = System.Diagnostics.Process.Start(pinfo);
					Processes.Add(process);
					var filename2 = file;
					var frames = Bitmaps.Count();
					process.Exited += (sender, args2) => {
						Errors.Message("Created {0} ({1}{2} MB RAM used)", filename2, (frames != 1) ? frames.ToString() + " frames, " : "", System.Environment.WorkingSet / (1024 * 1024));
					};
				} else {
					Errors.Message("Created {0} ({1}{2} MB RAM used)", Path.GetFileName(Filename), (Bitmaps.Count() != 1) ? Bitmaps.Count().ToString() + " frames, " : "", System.Environment.WorkingSet / (1024 * 1024));
				}
			}

			Thread.CurrentThread.CurrentCulture = oldculture;
			Thread.CurrentThread.CurrentUICulture = olduiculture;

			return this;
		}

		public static void SaveXps(List<System.Diagnostics.Process> Processes, Errors errors) {
			foreach (var key in XpsDocs.Keys.ToList()) {
				var doc = XpsDocs[key];

				var dir = Path.GetDirectoryName(key);
				if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
				if (File.Exists(key)) File.Delete(key);

				using (XpsDocument xpsd = new XpsDocument(key, FileAccess.ReadWrite)) {
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
						exe = "gxps-9.06-win32.exe";
					}
					var path = System.IO.Path.GetDirectoryName((new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath));
					if (path.StartsWith(System.Environment.GetFolderPath(Environment.SpecialFolder.Windows))) path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"\MSBuild\JohnsHope Software\XamlImageConverter\2.0");
					exe = Path.Combine(path, "gxps\\" + exe);
					//if (!File.Exists(exe)) exe = Path.Combine(path, @"\gxps.exe");
					var pinfo = new System.Diagnostics.ProcessStartInfo(exe);
					pinfo.CreateNoWindow = true;
					pinfo.Arguments =  string.Format("-sDEVICE={0} -dNOPAUSE \"-sOutputFile={1}\" \"{2}\"", device, filename, key);
					pinfo.UseShellExecute = false;
					pinfo.RedirectStandardOutput = true;
					pinfo.RedirectStandardError = true;
					pinfo.RedirectStandardInput = true;
					pinfo.WorkingDirectory = Path.GetDirectoryName(filename);
					var process = System.Diagnostics.Process.Start(pinfo);
					process.Exited += (sender, args) => {
						final = filename;
						var info = new FileInfo(final);
						errors.Message("Created {0} ({1} pages, {2:f2} MB)", info.Name, doc.Pages.Count, (double)info.Length / (1024 * 1024));
					};
					Processes.Add(process);
				} else {
					final = key;
					var info = new FileInfo(final);
					errors.Message("Created {0} ({1} pages, {2:f2} MB)", info.Name, doc.Pages.Count, (double)info.Length / (1024*1024));
				}
			}
		}

		/// <summary>
		/// Create a <see cref="BitmapEncoder"/> based on the target file extension
		/// </summary>
		/// <param name="filename">Target filename</param>
		/// <returns>Returns a new <see cref="BitmapEncoder"/> based on the target file extension</returns>
		public static IEncoder CreateEncoder(string filename, int? quality, double? dpi) {
			return EncoderFactory.Create(filename, quality, dpi);
		}

		/// <summary>
		/// Creates a film strip from a bitmap collection
		/// </summary>
		/// <param name="bitmaps">Collection of bitmaps</param>
		public static BitmapSource MakeFilmstrip(IEnumerable<BitmapSource> bitmaps, double? dpi) {
			var width = bitmaps.Max(b => b.PixelWidth);
			var stride = bitmaps.Max(b => b.PixelHeight);
			var height = stride * bitmaps.Count();
			var visual = new DrawingVisual();

			using (DrawingContext context = visual.RenderOpen()) {
				var offset = 0;

				foreach (var b in bitmaps) {
					context.DrawRectangle(new ImageBrush(b), null, new Rect(0, offset, width, stride));
					offset += stride;
				}
			}

			var target = new RenderTargetBitmap(width, height, dpi ?? 96, dpi ?? 96, PixelFormats.Pbgra32);
			target.Render(visual);

			return target;
		}

	}
}