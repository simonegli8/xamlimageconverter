using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;

namespace XamlImageConverter {


	public class FastBuildCheck {

		[NonSerialized]
		private static string ns = "http://schemas.johnshope.com/XamlImageConverter/2012";
		private XName xic(string name) { return XName.Get(name, ns); }

		public DateTime XicVersion = DateTime.MinValue, Version = DateTime.MinValue;
		public string Application = null;
		public Stack<string> OutputPaths = new Stack<string>();
		public string Culture = "";
		public System.Web.HttpContext Context;

		public string Filename(string file) {
			string path = "";
			foreach (var p in OutputPaths) {
				path = Path.Combine(p.Replace('/', '\\'), path);
				if (path[0] == '~') return Path.Combine(Application, path.Substring(1));
			}
			return path;
		}

		public FileInfo Last = null;
		public FileInfo InInfo(string file) { return Last = FileInfo(new Uri(file.Replace("~", Application)).LocalPath); }
		public FileInfo OutInfo(string file) { return Last = FileInfo(new Uri(Filename(file)).LocalPath); }

		public bool InFile(string file) {
			if (file.StartsWith("http://") || file.StartsWith("https://")) return DateTime.Now;
			var info = InInfo(file);
			if (info.Exists) {
				if (info.LastWriteTimeUtc > Version) Version = info.LastWriteTimeUtc;
				return true;
			}
			return false;
		}
		public bool OutFile(string file, string culure = null) {
			if (culture != null || string.IsNullOrEmpty(Culture)) {
				file = Path.ChangeExtension(file, culture + Path.GetExtension(file));
				var info = OutInfo(file);
				return (!info.Exists || info.LastWriteTimeUtc < Version);
			} else {
				return Culture.Split(';')
					.Select(c => c.Trim())
					.Where(c => !string.IsNullOrEmpty(c))
					.Any(c => OutFile(file, c));
			}
		}

		public class Group : IDisposable {
			public string OldCulture;
			public void Pop() { OutputPaths.Pop(); }
			public void Dispose() { Pop(); fbc.Culture = OldCulture; }
			FastBuildCheck FBC;
			public Group(FastBuildCheck fbc, string culture) {
				FBC = fbc;
				OldCulture = fbc.Culture;
				fbc.Culture = culture;
			}
		}

		public Group Open(XElement x) {
			if (x.Attribute("OutputPath") != null) OutputPaths.Push((string)x.Attribute("OutputPath"));
			return new Group(this, (string)x.Attribute("Cultures"));
		}

		public bool NeedsBuilding() {
			var file = context.Request.AppRelativeCurrentExecutionFilePath;
			if (file.EndsWith("axd")) return Doc(XamlScene.CreateXsd(Context.Request.QueryString));
			else if (file.EndsWith(".xic.xaml")) return DocFile(file);
			else if (file.EndsWith(".xaml")) return Doc(XamlScene.CreateDirect(file, Context.Request.QueryString));
			else return DocFile(System.IO.Path.GetFileNameWithoutExtension(filename));
		}

		bool DocFile(string filename) {
			if (!InFile(file)) return false;
			XicVersion = Version;
			var doc = XElement.Load(Last.FullName, LoadOptions.None);
			return Doc(doc);
		}

		bool Doc(XElement doc) {
			if (doc.Name != xic("XamlImageConverter")) return false;
			using (Open(doc)) return doc.Elements().Any(scene => ParseScene(scene));
		}

		bool ParseScene(XElement x) {
			if (x.Name != xic("Scene")) return false;
			using (Open(x)) {
				Version = XicVersion;
				string source = null;
				var xaml = scene.Elements(xic("Xaml")).SingleOrDefault();
				xaml = scene.Elements(xic("Source")).SingleOrDefault() ?? xaml;
				if (x.Attribute("Source") != null || x.Attribute("File") != null || x.Attribute("Type") != null) xaml = x;
				var srcAttr = xaml.Attribute("Source") ?? xaml.Attribute("File");
				if (srcAttr != null) source = srcAttr.Value;

				if (xaml == null || source == null) return false;
				if (x.Attribute("Dynamic") != null || InFile(source)) return true;

				ParseSteps(x.Elements());
			}
		}

		bool ParseSteps(XElement x) {
			using (Open(x)) {
				var groupOrSnapshot = x.Name == xic("Group") || x.Name == xic("Snapshot");
				var map = x.Name == xic("Map") || x.Name == xic("ImageMap");
				var par = x.Name == xic("Set") || x.Name == xic("Undo") || x.Name == xic("Reset");
				if (!(groupOrSnapshot || map || par)) return false;
				var file = (string)(x.Attribute("File") ?? x.Attribute("Filename"));
				if (OutFile(file)) return true;
				if (groupOrSnapshot) return x.Elements().Any(s => s.ParseSteps(s));
				return false;
			}
		}
	}
}