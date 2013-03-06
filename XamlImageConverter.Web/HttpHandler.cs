using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Reflection;
using System.Web;

namespace Silversite.Web {

	public class XamlBuildCheck {

		[NonSerialized]
		public static string ns = "http://schemas.johnshope.com/XamlImageConverter/2012";
		public XName xic(string name) { return XName.Get(name, ns); }
		public static string ns2 = "http://www.chriscavanagh.com/SkinBuilder";

		public DateTime XicVersion = DateTime.MinValue, Version = DateTime.MinValue;
		public string Application { get { return Context.Request.PhysicalApplicationPath; } }
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
		public string Filename2(string file) {
			if (!file.Contains(":") && !file.Contains("~")) {
				if (file.StartsWith("/")) file = "~/" + file.Substring(System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath.Length);
				else file = Path.Combine(Path.GetDirectoryName(Context.Request.AppRelativeCurrentExecutionFilePath.Replace('/','\\')), file);
			}
			file = file.Replace('/', '\\').Replace("~\\", Application);
			return file;
		}
		public FileInfo Last = null;
		public FileInfo InInfo(string file) {
            return Last = new FileInfo(Filename2(file));
		}
		public FileInfo OutInfo(string file) { return Last = new FileInfo(Filename2(Filename(file))); }

		public bool InFile(string file) {
			if (file.StartsWith("http://") || file.StartsWith("https://")) return true;
			var info = InInfo(file);
			if (info.Exists) {
				if (info.LastWriteTimeUtc > Version) Version = info.LastWriteTimeUtc;
				return true;
			}
			return false;
		}
		public bool OutFile(string file, string culture = null) {
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
			public void Pop() { if (FBC.OutputPaths.Count > 0) FBC.OutputPaths.Pop(); }
			public void Dispose() { Pop(); FBC.Culture = OldCulture; }
			XamlBuildCheck FBC;
			public Group(XamlBuildCheck fbc, string culture) {
				FBC = fbc;
				OldCulture = fbc.Culture;
				fbc.Culture = culture;
			}
		}

		public Group Open(XElement x) {
			if (x.Attribute("OutputPath") != null) OutputPaths.Push((string)x.Attribute("OutputPath"));
			return new Group(this, (string)x.Attribute("Cultures"));
		}

		public Dictionary<string, string> QueryString {
			get {
				var query = HttpContext.Current.Request.QueryString;
				var dict = new Dictionary<string, string>();
				foreach (string key in query.Keys) dict.Add(key, query[key]);
				return dict;
			}
		}

		public bool NeedsBuilding() {
			var file = HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath;

			if (file.EndsWith(".axd")) {
				var xaml = Context.Request.QueryString["Xaml"];
                if (string.IsNullOrEmpty(xaml)) return false;
				if (!string.IsNullOrEmpty(xaml) && xaml.Trim()[0] == '#') xaml = (string)HttpContext.Current.Session["XamlImageConverter.Xaml:" + xaml];
                if (xaml.Trim()[0] == '<') {
					using (var r = new StringReader(xaml)) {
						var xdoc = XElement.Load(r, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);
						if (xdoc.Name.LocalName == "XamlImageConverter" && (xdoc.Name.NamespaceName == ns || xdoc.Name.NamespaceName == ns2)) return Doc(xdoc);
						else return Doc(XamlScene.CreateDirect(xdoc, QueryString));
					}
				}
				return Doc(XamlScene.CreateXsd(QueryString));
			} else if (file.EndsWith(".xic.xaml")) return DocFile(file);
			else if (file.EndsWith(".xaml")) return Doc(XamlScene.CreateDirect(file, QueryString));
			else return Doc(XamlScene.CreateDirect(System.IO.Path.GetFileNameWithoutExtension(file), QueryString));
		}

		bool DocFile(string filename) {
			if (!InFile(filename)) return false;
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
				var xaml = x.Elements(xic("Xaml")).SingleOrDefault();
				xaml = x.Elements(xic("Source")).SingleOrDefault() ?? xaml;
				if (x.Attribute("Source") != null || x.Attribute("File") != null || x.Attribute("Type") != null) xaml = x;
				var srcAttr = xaml.Attribute("Source") ?? xaml.Attribute("File");
				if (srcAttr != null) source = srcAttr.Value;

				if (xaml == null || source == null) return false;
				if (x.Attribute("Dynamic") != null || InFile(source)) return true;

				return x.Elements().Any(s => ParseSteps(s));
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
				if (groupOrSnapshot) return x.Elements().Any(s => ParseSteps(s));
				return false;
			}
		}
	}


	public class XamlScene {

		static XNamespace xic = XamlBuildCheck.ns;
		static string ns = XamlBuildCheck.ns;
		static string ns2 = XamlBuildCheck.ns2;


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
				if(x.Attributes().Any(a => a.Name.NamespaceName == ns || a.Name.NamespaceName == ns2)) {
					var par = new Dictionary<string, string>();
					foreach (var p in x.Attributes().Where(a =>a.Name.NamespaceName ==ns || a.Name.NamespaceName == ns2)) {
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
	public class Cache {

		public static string Path { get; set; }

		static XNamespace ns = XamlBuildCheck.ns;

		public static string File(string id, XElement xaml, string type = null, string path = null) {
			var e = xaml.DescendantsAndSelf(ns + id).FirstOrDefault();
			if (e != null) {
				if (e.Attribute(ns + "ImageType") != null) type = (string)e.Attribute(ns + "ImageType");
				if (e.Attribute(ns + "Cache") != null) path = (string)e.Attribute(ns + "Cache");
			}
			path = path ?? Path;
			if (type == null) type = "png";
			if (!path.EndsWith("/")) path += "/";
			return path + id + "." + Hash.Compute(xaml).ToString() + "." + type;
		}
	}

#if !Silversite
	public class Hash {

		public static int Compute(byte[] data) {
			unchecked {
				const int p = 16777619;
				int hash = (int)2166136261;

				for (int i = 0; i < data.Length; i++)
					hash = (hash ^ data[i]) * p;

				hash += hash << 13;
				hash ^= hash >> 7;
				hash += hash << 3;
				hash ^= hash >> 17;
				hash += hash << 5;
				hash &= 0x7FFFFFFF;
				return hash;
			}
		}

		public static int Compute(string s) { return Compute(System.Text.Encoding.UTF8.GetBytes(s)); }

		public static int Compute(XElement e) {
			using (var m = new MemoryStream())
			using (var w = XmlWriter.Create(m, new XmlWriterSettings() { NewLineChars = "", Encoding = Encoding.UTF8, Indent = false })) {
				e.Save(w);
				return Compute(m.GetBuffer());
			}
		}
	}

#else
			public class Hash: Silversite.Services.Hash {
			}
#endif

#if Silversite
	[Configuration.ConfigurationSection(Name = "XamlImageConverter")]
	public class Configuration : Silversite.Configuration.Section {
		[ConfigurationProperty("UseService", IsRequired = false, DefaultValue = false)]
		public bool UseSevice { get { return (bool)(this["UseService"] ?? true); } set { this["UseService"] = value; } }
		[ConfigurationProperty("Log", IsRequired = false, DefaultValue = true)]
		public bool Log { get { return (bool)(this["Log"] ?? true); } set { this["Log"] = value; } }
	}
#endif

	public class XamlImageHandler : System.Web.IHttpHandler {

#if Silversite
		public static Configuration Configuration = new Configuration();
#endif

		public bool IsReusable { get { return true; } }

		static System.Web.IHttpHandler handler = null;

		public void ProcessRequest(System.Web.HttpContext context) {
			
			var checker = new XamlBuildCheck() { Context = context };
			
			if (handler != null || checker.NeedsBuilding()) {
				if (handler == null) {
					try {
#if Silversite
						var handlerInfo = Services.Lazy.Types.Info("XamlImageConverter.XamlImageHandler");
						handlerInfo.Load();
						handler = handlerInfo.New();
#else
						var a = Assembly.LoadFrom(context.Server.MapPath("~/Bin/Lazy/XamlImageConverter.dll"));
						var type = a.GetType("XamlImageConverter.XamlImageHandler");
						handler = (System.Web.IHttpHandler)Activator.CreateInstance(type);
#endif
					} catch {
						context.Response.StatusCode = 500;
						context.Response.End();
					}
				}
#if Silversite
				context.Application.Lock();
				context.Application["XamlImageConverter.Configuration.UseService"] = Configuration.UseService;
				context.Application["XamlImageConverter.Configuration.Log"] = Configuration.Log;
				context.Application.UnLock();
#endif
				handler.ProcessRequest(context);
			} else {
				var filename = context.Request.AppRelativeCurrentExecutionFilePath;
				string image = context.Request.QueryString["Image"];
				if (!string.IsNullOrEmpty(image)) filename = image;
				var sfile = context.Server.MapPath(filename);
				switch (System.IO.Path.GetExtension(filename).ToLower()) {
				case ".bmp": context.Response.ContentType = "image/bmp"; break;
				case ".png": context.Response.ContentType = "image/png"; break;
				case ".jpg":
				case ".jpeg":	context.Response.ContentType = "image/jpeg"; break;
				case ".tif":
				case ".tiff": context.Response.ContentType = "image/tiff"; break;
				case ".gif": context.Response.ContentType = "image/gif"; break;
				case ".wdp": context.Response.ContentType = "image/vnd.ms-photo"; break;
				case ".pdf": 
					context.Response.AppendHeader("content-disposition", "attachment; filename=" + System.IO.Path.GetFileName(sfile));
					context.Response.ContentType = "application/pdf"; break;
				case ".xps":
					context.Response.AppendHeader("content-disposition", "attachment; filename=" + System.IO.Path.GetFileName(sfile));
					context.Response.ContentType = "application/vnd.ms-xpsdocument"; break;
				case ".eps":
				case ".ps":
					context.Response.AppendHeader("content-disposition", "attachment; filename=" + System.IO.Path.GetFileName(sfile));
					context.Response.ContentType = "application/postscript"; break;
				default: break;
				}

				if (System.IO.File.Exists(sfile)) context.Response.WriteFile(sfile);
				else {
					context.Response.StatusCode = 404;
					context.Response.End();
				}
			}
		}
	}
}