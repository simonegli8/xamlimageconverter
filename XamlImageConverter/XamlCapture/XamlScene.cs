using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;

namespace XamlImageConverter {

	public static class XamlScene {

		static XNamespace xic = Parser.xic;
		static XNamespace sb = Parser.sb;
		static string[] validAttributes = new string[] { "Element", "Storyboard", "Frames", "Filmstrip", "Dpi", "RenderDpi", "Quality", "Filename", "Left", "Top", "Right", "Bottom", "Width", "Height", "Cultures", "RenderTimeout", "Page", "FitToPage", "File", "Loop", "Pause", "Skin", "Theme", "Type", "Image" };

		public static void ApplyParameters(Compiler compiler, XElement e, string filename, Dictionary<string, string> parameters) {
			var type = "png";
			int hash = 0;
			bool addhash = false, nohash = false;

			foreach (var key in parameters.Keys.ToList()) {

				if (validAttributes.Any(a => a == key)) {
					if (key == "Image" || key == "File" || key == "Filename") nohash = true;
					else if (key == "Type") type = parameters["Type"];
					else {
						addhash = !nohash;
						hash += Hash.Compute(key + "=" + parameters[key]);
					}
					e.SetAttributeValue(key, parameters[key]);
					parameters.Remove(key);
					addhash = !nohash;
				} else {
					addhash = !nohash;
					hash += Hash.Compute(key + "=" + parameters[key]);
				}
			}
			if (addhash) {
				compiler.hash = hash;
				e.SetAttributeValue("Hash", hash);
			}
			if (!nohash && filename != null) {
				e.SetAttributeValue("File", filename + "." + type);
			}
		}

		public static XElement CreateDirect(Compiler compiler, string filename, Dictionary<string, string> parameters) {
			if (filename.EndsWith(".xic.xaml")) {
				foreach (var key in validAttributes) parameters.Remove(key);
				return XElement.Load(compiler.MapPath(filename), LoadOptions.PreserveWhitespace | LoadOptions.SetBaseUri | LoadOptions.SetLineInfo);
			}
			XElement snapshot;
			var res = new XElement(xic + "XamlImageConverter",
					new XElement(xic + "Scene",
						new XAttribute("Source", filename),
						snapshot = new XElement(xic + "Snapshot")
					)
				);
			ApplyParameters(compiler, snapshot, null, parameters);
			return res;
		}
		
		public static XElement CreateDirect(Compiler compiler, string filename, XElement e, Dictionary<string, string> parameters) {
			XElement scene;
			if (e.Name == xic + "XamlImageConverter") return e;
			var res = new XElement(xic + "XamlImageConverter",
					scene = new XElement(xic + "Scene", new XElement(xic  + "Xaml", e))
				);
			if (parameters.ContainsKey("Image") || parameters.ContainsKey("File") || parameters.ContainsKey("Filename") || parameters.ContainsKey("Type")) {
				var snapshot = new XElement(xic + "Snapshot");
				ApplyParameters(compiler, snapshot, filename, parameters);
				scene.Add(snapshot);
			}

			return res;
		}

		public static XElement CreateAxd(Compiler compiler, Dictionary<string, string> par) {
			var src = par["Source"];
			if (string.IsNullOrEmpty(src)) {
				compiler.Errors.Error("Source cannot be empty.", "32", new TextSpan());
				return null;
			}
			if (src.Trim()[0] == '#') src = (string)System.Web.HttpContext.Current.Session["XamlImageConverter.Xaml:" + src];
			if (!src.Trim().StartsWith("<")) return CreateDirect(compiler, src, par);
			return CreateDirect(compiler, XamlImageHandler.Cache + "/" + Hash.Compute(src).ToString("X") , XElement.Parse(src, LoadOptions.PreserveWhitespace | LoadOptions.SetBaseUri | LoadOptions.SetLineInfo), par);
		}
	}
}
