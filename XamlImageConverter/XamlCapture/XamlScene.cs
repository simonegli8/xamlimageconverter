using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace XamlImageConverter {

	public class XamlScene {

		static XNamespace xic = Parser.ns1;

		public static void ApplyParameters(XElement e, Dictionary<string, string> parameters) {
			var validAttributes = new string[] { "Element", "Storyboard", "Frames", "Filmstrip", "Dpi", "RenderDpi", "Quality", "Filename", "Left", "Top", "Right", "Bottom", "Width", "Height", "Cultures", "RenderTimeout", "Page", "FitToPage", "File", "Loop", "Pause", "Skin", "Theme" };

			foreach (var p in parameters) {
				if (validAttributes.Any(a => a == p.Key)) e.SetAttributeValue(p.Key, p.Value);
			}
			if (parameters.ContainsKey("Image")) e.SetAttributeValue("File", parameters["Image"]);
		}

		public static XElement CreateDirect(string filename, Dictionary<string, string> parameters) {
			var xamlfile = filename;
			if (!filename.EndsWith(".xaml")) xamlfile = System.IO.Path.GetFileNameWithoutExtension(filename);
			else filename = filename + ".png"; 
			XElement snapshot;
			var res = new XElement(xic + "XamlImageConverter",
					new XElement(xic + "Scene",
						new XAttribute("Source", xamlfile),
						snapshot = new XElement(xic + "Snapshot", new XAttribute("File", filename))
					)
				);
			ApplyParameters(snapshot, parameters);
			return res;
		}
		
		public static XElement CreateDirect(XElement e, Dictionary<string, string> parameters) {
			XElement scene;
			var res = new XElement(xic + "XamlImageConverter",
					scene = new XElement(xic + "Scene", new XAttribute("Xaml", e))
				);
			if (parameters.ContainsKey("Image")) {
				var snapshot = new XElement(xic + "Snapshot");
				ApplyParameters(snapshot, parameters);
				scene.Add(snapshot);
			}
			foreach (var x in e.Descendants()) {
				if(x.Attributes().Any(a => a.Name.NamespaceName == Parser.ns1 || a.Name.NamespaceName == Parser.ns2)) {
					var par = new Dictionary<string, string>();
					foreach (var p in x.Attributes().Where(a =>a.Name.NamespaceName == Parser.ns1 || a.Name.NamespaceName == Parser.ns2)) {
						if (System.IO.Path.GetExtension(p.Name.LocalName) == ".View") {
							string type, cache;
							parameters.TryGetValue("ImageType", out type);
							parameters.TryGetValue("Cache", out cache);
							par.Add("File", Cache.File(p.Name.LocalName, e, type, cache));
						} else par.Add(p.Name.LocalName, p.Value);
					}
					var snapshot = new XElement(xic + "Snapshot");
					ApplyParameters(snapshot, par);
					scene.Add(snapshot);
				}
			}
			return res;
		}

		public static XElement CreateXsd(Dictionary<string, string> parameters) {
			var filename = parameters["file"];
			XElement snapshot;
			var xaml = parameters["xaml"];
			var res = new XElement(xic + "XamlImageConverter",
					new XElement(xic + "Scene",
						new XElement("Xaml", xaml),
						snapshot = new XElement(xic + "Snapshot", new XAttribute("File", filename))
					)
				);
			ApplyParameters(snapshot, parameters);
			return res;
		}
	}
}
