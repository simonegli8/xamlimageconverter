using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Windows.Media;
using System.Reflection;
using System.Threading;
using System.Globalization;

namespace XamlImageConverter {

	[Serializable]
	public class Parser: MarshalByRefObject {

		[NonSerialized]
		Errors errors = null;
		public Errors Errors { get { if (errors == null) errors = new Errors(); return errors; } set { errors = value; } }

		public Compiler Compiler { get { return this as Compiler; } } 

		[NonSerialized]
		public static string ns1 = "http://schemas.johnshope.com/XamlImageConverter/2012";
		[NonSerialized]
		public static string ns2 ="http://www.chriscavanagh.com/SkinBuilder";

		public void ParseStyle(XElement e, Group group) {
			group.Theme = (string)e.Attribute("Theme");
			group.Skin = (string)e.Attribute("Skin");
			if (group.Theme == "") group.Theme = null;
			if (group.Skin == "") group.Skin = null;
			if (e.Attribute("TextMode") != null) {
				var modes = ((string)e.Attribute("TextMode")).Split('.', ',', ';').Select(m => m.Trim());
				foreach (var mode in modes) {
					TextFormattingMode formatting;
					TextRenderingMode rendering;
					if (Enum.TryParse<TextFormattingMode>(mode, out formatting)) group.TextFormattingMode = formatting;
					if (Enum.TryParse<TextRenderingMode>(mode, out rendering)) group.TextRenderingMode = rendering;
				}
			}
		}

		/// <summary>
		/// Save snapshots for multiple scenes
		/// </summary>
		/// <param name="config">Configuration element</param>
		/// <returns>Returns a collection of snapshot bitmaps</returns>
		public IEnumerable<Group> ParseScenes(DateTime Version, XElement config) {
			var Root = new Group();
			Root.XElement = config;
			Root.Compiler = Compiler;
			var ns = config.Name.NamespaceName;
			if (ns == ns1 || ns == ns2) {
				Namespace = ns;
				if (config.Name != SBName("XamlImageConverter")) Errors.Error("Invalid root element", "26", config);
				ParseStyle(config, Root);
				Root.OutputPath = (string)config.Attribute("OutputPath");

				foreach (var x in config.Elements()) {
					if (x.Name == SBName("Scene")) {
						Group scene = null;
						try {
							scene = ParseScene(Version, x);
						} catch (CompilerInnerException ex) {
							Errors.Error(ex.Message + "\n" + ex.InnerException.Message + "\n" + ex.InnerException.StackTrace, ex.ErrorCode.ToString(), ex.XObject);
						} catch (CompilerException ex) {
							Errors.Error(ex.Message, ex.ErrorCode.ToString(), ex.XObject);
						} catch (Exception ex) {
							Errors.Warning("An internal parsing error occurred\n\n" + ex.Message + "\n" + ex.StackTrace, "2", x);
						}
						if (scene != null) {
							Root.Children.Add(scene);
							yield return scene;
						}
					} else {
						Errors.Error("Invalid element " + x.Name.LocalName, "24", x);
					}
				}
			} else {
				Errors.Error("Invalid namespace " + config.Name.NamespaceName, "25", config);
			}
		}

		/// <summary>
		/// Returns a collection of <see cref="Snapshot"/>s from configuration elements
		/// </summary>
		/// <param name="snapshots">A collection of snapshot configuration elements</param>
		/// <returns>Returns a list of <see cref="Snapshot"/>s</returns>
		public Group ParseScene(DateTime Version, XElement x) {
			var scene = new Group() { Compiler = Compiler, XElement = x, IsScene = true };

			var xaml = x.Elements(SBName("Xaml")).SingleOrDefault();
			xaml = x.Elements(SBName("Source")).SingleOrDefault() ?? xaml;
			if (x.Attribute("Source") != null || x.Attribute("File") != null || x.Attribute("Type") != null) xaml = x;
			var srcAttr = xaml.Attribute("Source") ?? xaml.Attribute("File");
			if (srcAttr != null) scene.Source = srcAttr.Value;
			var src = scene.Source;

			if (xaml == null) throw new CompilerException("Scene must contain source file specification.", 10, x);
			scene.XamlElement = xaml;

			var width = (double?)xaml.Attribute("Width") ?? double.PositiveInfinity;
			var height = (double?)xaml.Attribute("Height") ?? double.PositiveInfinity;
			scene.Dependencies = (string)x.Attribute("DependsOn");
			scene.CulturesString = (string)x.Attribute("Cultures");
			scene.OutputPath = (string)x.Attribute("OutputPath");
			scene.PreferredSize = new Size(width, height);

			ParseStyle(x, scene);

			DateTime XamlVersion = new DateTime();
			FileInfo info;

			if (src != null) {
				info = new FileInfo(Compiler.MapPath(src));
				if (!info.Exists) throw new CompilerException("Source file " + info.FullName + " not found.", 11, xaml);

				XamlVersion = info.LastWriteTimeUtc;
				if (XamlVersion > Version) Version = XamlVersion;
			} else {
				string assemblyName = (string)xaml.Attribute("Assembly");
				string typename = (string)xaml.Attribute("Type");
				
				scene.AssemblyName = assemblyName;
				scene.TypeName = typename;

				if (typename == null) {
					scene.InnerXaml = xaml.Elements().SingleOrDefault();
					if (scene.InnerXaml == null) throw new CompilerException("Scene must contain a single XAML root element", 16, xaml);
				}
			}

			if (xaml.Attribute("Dynamic") != null && xaml.Attribute("Dynamic").Value == "true") Version = DateTime.Now.ToUniversalTime();
	

			// parse dependencies
			foreach (var dependency in x.Elements().Where(child => child.Name == SBName("Depends"))) {
				var d = new Dependency(dependency) { Compiler = Compiler };
				if (d.Version > Version) Version = d.Version;
			}

			scene.Version = Version;

			if (Compiler.Parameters.Count > 0) {
				var p = new Parameters(Compiler.Parameters);
				p.Compiler = Compiler;
				p.ElementName = string.Empty;
				p.XElement = null;
				scene.Children.Add(p);
			}

			// parse oridnary elements
			var names = new XName[] { SBName("Xaml"), SBName("Depends") };
			foreach (var child in x.Elements().Where(child => !names.Contains(child.Name))) {
				Parse(child, scene);
			}

			foreach (var node in x.Nodes().Where(node => !(node is XElement || node is XComment ||
				(node is XText && (string.IsNullOrWhiteSpace(((XText)node).Value) || node.NodeType == System.Xml.XmlNodeType.Whitespace || node.NodeType == System.Xml.XmlNodeType.SignificantWhitespace))))) {
				Errors.Error(string.Format("Invalid content {0}", node.ToString(SaveOptions.DisableFormatting | SaveOptions.OmitDuplicateNamespaces)), "23", node);
			}

			return scene;
		}


		/// <summary>
		/// Returns a collection of <see cref="Snapshot"/>s from configuration elements
		/// </summary>
		/// <param name="snapshots">A collection of snapshot configuration elements</param>
		/// <returns>Returns a list of <see cref="Step"/>s</returns>
		public Group Parse(XElement x, Group container) {
			Group item = CreateItem(container, x);
			item.Version = container.Version;
			container.Children.Add(item);

			if (x.Attribute("Left") != null) item.SetValue(Canvas.LeftProperty, (double)x.Attribute("Left"));
			if (x.Attribute("Top") != null) item.SetValue(Canvas.TopProperty, (double)x.Attribute("Top"));
			if (x.Attribute("Right") != null) item.SetValue(Canvas.RightProperty, (double)x.Attribute("Right"));
			if (x.Attribute("Bottom") != null) item.SetValue(Canvas.BottomProperty, (double)x.Attribute("Bottom"));
			if (x.Attribute("Width") != null) item.Width = (double)x.Attribute("Width");
			if (x.Attribute("Height") != null) item.Height = (double)x.Attribute("Height");
			if (x.Attribute("Cultures") != null) item.CulturesString = (string)x.Attribute("Cultures");

			ParseStyle(x, item);

			if (item.ParseChildren) {
				foreach (var child in x.Elements()) {
					Parse(child, item);
				}
				foreach (var node in x.Nodes().Where(node => !(node is XElement || node is XComment ||
					(node is XText && (string.IsNullOrWhiteSpace(((XText)node).Value) || node.NodeType == System.Xml.XmlNodeType.Whitespace || node.NodeType == System.Xml.XmlNodeType.SignificantWhitespace))))) {
					Errors.Error(string.Format("Invalid content {0}", node.ToString(SaveOptions.DisableFormatting | SaveOptions.OmitDuplicateNamespaces)), "23", node);
				}
			}

			return item;
		}

		public void ValidAttributes(XElement x, params string[] names) {
			foreach (var a in x.Attributes()) {
				if (!names.Contains(a.Name.LocalName)) {
					Errors.Error("Invalid attribute " + a.Name.LocalName, "22", a);
				}
			}
		}

		/// <summary>
		/// Create a new group or snapshot
		/// </summary>
		/// <param name="container">Container element</param>
		/// <param name="x">Definition element</param>
		/// <returns>Returns a new group or snapshot</returns>
		private Group CreateItem(Group container, XElement x) {
			Group result = null;

			switch (x.Name.LocalName) {
			case "Snapshot":
				result = new Snapshot {
					StoryboardName = (string)x.Attribute("Storyboard"),
					Frames = (int?)x.Attribute("Frames"),
					Filmstrip = (bool?)x.Attribute("Filmstrip") ?? false,
					Dpi = (double?)x.Attribute("Dpi"),
					Quality = (int?)x.Attribute("Quality"),
					Filename = (string)x.Attribute("Filename") ?? (string)x.Attribute("File"),
					CulturesString = (string)x.Attribute("Cultures"),
					Page = (string)x.Attribute("Page"),
					FitToPage = (bool?)x.Attribute("FitToPage") ?? false,
					Loop = (int?)x.Attribute("Loop") ?? 1,
					Pause = (double?)x.Attribute("Pause") ?? 0
				};
				ValidAttributes(x, "Element", "Storyboard", "Frames", "Filmstrip", "Dpi", "Quality", "Filename", "Left", "Top", "Right", "Bottom", "Width", "Height", "Cultures", "Page", "FitToPage", 
					"File", "Loop", "Pause", "Skin", "Theme", "TextMode");
				break;
			case "ImageMap":
			case "Map":
				var map = new ImageMap {
					Image = (string)x.Attribute("Image"),
 					Scale = (double?)x.Attribute("Scale") ?? 1,
					Filename = (string)x.Attribute("Filename") ?? (string)x.Attribute("File"),
					Flatness = (double?)x.Attribute("Flatness") ?? 0.5,
					Dpi = (double?)x.Attribute("Dpi") ?? 96.0
				};
				map.Type = x.Name.LocalName == "Map" ? ImageMap.Types.Html : ImageMap.Types.AspNet;

				switch (Path.GetExtension(map.Filename)) {
					case ".ascx": map.FileType = ImageMap.FileTypes.UserControl; map.Type = ImageMap.Types.AspNet; break;
					case ".aspx": map.FileType = ImageMap.FileTypes.Insert; map.Type = ImageMap.Types.AspNet; break;
					default: map.FileType = ImageMap.FileTypes.Insert; map.Type = ImageMap.Types.Html; break;
				}
				var fileType = (string)x.Attribute("FileType");
				if (fileType != null) map.FileType = (ImageMap.FileTypes)Enum.Parse(typeof(ImageMap.FileTypes), fileType);
				var type = (string)x.Attribute("Type");
				if (type != null) map.Type = (ImageMap.Types)Enum.Parse(typeof(ImageMap.Types), type);
				var ident = (string)x.Attribute("Ident");
				if (ident != null) map.Ident = (ImageMap.IdentChars)Enum.Parse(typeof(ImageMap.IdentChars), ident);

				var predefined = new string[] { "Image", "Scale", "Filename", "File", "Type", "Flatness", "FileType", "Dpi", "Ident" };
				foreach (var attribute in x.Attributes().Where(a => predefined.All(p => p != a.Name))) map.Attributes.Add(attribute);

				map.Areas.AddRange(x.Elements());
				result = map;
				break;
			case "Set":
				result = new Parameters(x);
				break;
			case "Undo":
				result = new Undo();
				ValidAttributes(x);
				break;
			case "Reset":
				result = new Reset();
				ValidAttributes(x);
				break;
			case "Group":
				result = new Group { OutputPath = (string)x.Attribute("OutputPath") };
				ValidAttributes(x, "Element", "OutputPath", "Left", "Top", "Right", "Bottom", "Width", "Height", "Skin", "Theme", "TextMode");
				break;
			default:
				Errors.Error("Invalid element " +  x.Name.LocalName, "20", x);
				result = new Group();
				break;
			}
			result.Compiler = Compiler;
			result.ElementName = (string)x.Attribute("Element");
			result.XElement = x;

			return result;
		}

		private string Namespace = ns1;
		/// <summary>
		/// Returns a namespace qualified <see cref="XName"/>
		/// </summary>
		/// <param name="name">Local name</param>
		/// <returns>Returns a namespace qualified <see cref="XName"/></returns>
		private XName SBName(string name) {
			return XName.Get(name, Namespace);
		}
	}
}