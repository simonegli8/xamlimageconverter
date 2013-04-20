using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Reflection;
using System.Web;
#if Silversite
using System.Configuration;
#endif


namespace XamlImageConverter.Web {

	public class XamlBuildCheck {

		[NonSerialized]
		public static XNamespace xic = "http://schemas.johnshope.com/XamlImageConverter/2012";
		public static XNamespace sb = "http://www.chriscavanagh.com/SkinBuilder";
		public static XNamespace xamlns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
		public static XNamespace xxamlns = "http://schemas.microsoft.com/winfx/2006/xaml";

		public static XNamespace ns;

		public DateTime nsVersion = DateTime.MinValue, Version = DateTime.MinValue;
		public string Application { get { return Context.Request.PhysicalApplicationPath; } }
		public Stack<string> OutputPaths = new Stack<string>();
		public string Culture = "";
		public System.Web.HttpContext Context;
		public int? hash;

		public string OutFilename(string file) {
			if (!file.Contains(":") && !file.Contains("~")) {
				string path = "";
				foreach (var p in OutputPaths) {
					path = Path.Combine(p.Replace('/', '\\'), path);
					if (path.Length > 0 && path[0] == '~') return new FileInfo(Path.Combine(Application, path.Substring(1), file.Replace('/', '\\'))).FullName;
				}
				return InFilename(Path.Combine(path, file));
			} else return InFilename(file);
		}
		public string InFilename(string file) {
			if (!file.Contains(":") && !file.Contains("~")) {
				if (file.StartsWith("/")) file = "~/" + file.Substring(System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath.Length);
				else file = Path.Combine(Path.GetDirectoryName(Context.Request.AppRelativeCurrentExecutionFilePath.Replace('/','\\')), file);
			}
			file = file.Replace('/', '\\').Replace("~\\", Application);
			return new FileInfo(file).FullName;
		}
		public FileInfo Last = null;
		public FileInfo InInfo(string file) {
				return Last = new FileInfo(InFilename(file));
		}
		public FileInfo OutInfo(string file) { return Last = new FileInfo(OutFilename(file)); }

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
				return Culture.Split(',', ';')
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
			else OutputPaths.Push("");
			return new Group(this, (string)x.Attribute("Cultures"));
		}

		public Dictionary<string, string> QueryString {
			get {
				var query = HttpContext.Current.Request.QueryString;
				var dict = new Dictionary<string, string>();
				foreach (string key in query.Keys) {
					if (key != null) dict.Add(key, query[key]);
				}
				var exts = query.GetValues(null);
				if (exts != null && exts.Length > 0) dict.Add("Type", exts[0]);
				return dict;
			}
		}

		public bool NeedsBuilding() {
			var file = HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath;
			var ext = Path.GetExtension(file).ToLower();

			if (ext == ".axd") {
				return Doc(CreateAxd(QueryString));
			} else if (ext == ".xaml" || ext == ".psd" || ext == ".svg" || ext == ".svgz") {
				if (!InFile(file)) return false;
				nsVersion = Version;
				return Doc(CreateDirect(file, QueryString));
			} else return Doc(CreateDirect(System.IO.Path.GetFileNameWithoutExtension(file), QueryString));
		}

		bool DocFile(string filename) {
			if (!InFile(filename)) return false;
			nsVersion = Version;
			var doc = XElement.Load(Last.FullName, LoadOptions.None);
			using (Open(doc)) return Doc(doc);
		}

		bool Doc(XElement doc) {
			if (doc == Dynamic) return DynamicResult;
			ns = doc.Name.Namespace;
			if (doc.Name != xic+"XamlImageConverter" && doc.Name != sb+"SkinBuilder") return false;
			using (Open(doc)) return doc.Elements().Any(scene => ParseScene(scene));
		}

		bool ParseScene(XElement x) {
			if (x.Name != ns+"Scene") return false;
			using (Open(x)) {
				Version = nsVersion;
				string source = null;
				var xaml = x.Elements(ns+"Xaml").SingleOrDefault();
				xaml = x.Elements(ns+"Source").SingleOrDefault() ?? xaml;
				if (x.Attribute("Source") != null || x.Attribute("File") != null || x.Attribute("Type") != null) xaml = x;
				var srcAttr = xaml.Attribute("Source") ?? xaml.Attribute("File");
				if (srcAttr != null) source = srcAttr.Value;

				if (xaml == null && source == null) return false;
				if (x.Attribute("Dynamic") != null && InFile(source)) return true;

				return x.Elements().Any(s => ParseSteps(s, source));
			}
		}

		bool ParseSteps(XElement x, string source) {
			using (Open(x)) {
				var snapshot = x.Name == ns+"Snapshot";
				var groupOrSnapshot = x.Name == ns+"Group" || snapshot;
				var map = x.Name == ns+"Map" || x.Name == ns+"ImageMap";
				var par = x.Name == ns+"Set" || x.Name == ns+"Undo" || x.Name == ns+"Reset";
				if (!(groupOrSnapshot || map || par)) return false;
				var file = (string)(x.Attribute("File") ?? x.Attribute("Filename") ?? x.Attribute("Image"));
				if (snapshot && file == null) {
					var type = x.Attribute("Type");
					if (type != null) file = source + "." + type.Value;
				}
				if (snapshot && OutFile(file)) return true;
				if (groupOrSnapshot) return x.Elements().Any(s => ParseSteps(s, source));
				return false;
			}
		}

		public XElement Dynamic = new XElement(xic+"Dynamic");
		public bool DynamicResult;
		public static string[] validAttributes = new string[] { "Element", "Storyboard", "Frames", "Filmstrip", "Dpi", "RenderDpi", "Quality", "Filename", "Left", "Top", "Right", "Bottom", "Width", "Height", "Cultures", "RenderTimeout", "Page", "FitToPage", "File", "Loop", "Pause", "Skin", "Theme", "Type", "Image" };

		public void ApplyParameters(string filename, XElement e, Dictionary<string, string> parameters) {

			var type = "png";
			int? h = 0;
			bool nohash = false;

			foreach (var key in parameters.Keys.ToList()) {

				if (validAttributes.Any(a => a == key)) {
					if (key == "Image" || key == "File" || key == "Filename") nohash = true;
					else if (key == "Type") type = parameters["Type"];
					else {
						h = (h ?? 0) + Hash.Compute(key + "=" + parameters[key]);
					}
					e.SetAttributeValue(key, parameters[key]);
					parameters.Remove(key);
				} else {
					h = (h ?? 0) + Hash.Compute(key + "=" + parameters[key]);
					if (key == "Source") parameters.Remove(key);
				}
			}
			if (h != null && !nohash) {
				hash = h;
				e.SetAttributeValue("Hash", hash);
			}
			if (!nohash && filename != null) {
				e.SetAttributeValue("File", filename + "." + type);
			}

			foreach (var key in parameters.Keys.ToList()) {
				if (validAttributes.Any(a => a == key)) {
					if (key == "Image" || key == "File" || key == "Filename") nohash = true;
					else if (key == "Type") type = parameters["Type"];
					e.SetAttributeValue(key, parameters[key]);
					parameters.Remove(key);
				}
			}
			if (parameters.Count > 0 && !nohash) {
				hash = 0;
				foreach (var key in parameters.Keys) {
					hash += Hash.Compute(key + "=" + parameters[key]);
				}
				e.SetAttributeValue("Hash", hash.Value);
			}
			if (!nohash && filename != null) {
				e.SetAttributeValue("File", filename + "." + type);
			}
		}

		public static void ParseXaml(XElement xaml, XElement scene) {
			var isnapshots = xaml.DescendantsAndSelf()
				.Where(e => e.Attributes().Any(a => a.Name.Namespace == xic));
			foreach (var isn in isnapshots) {
				var name = isn.Attribute(xxamlns+"Name") ?? isn.Attribute("Name");
				if (name == null && isn != xaml) continue;
				var sn = new XElement(xic+"Snapshot");
				if (name != null) sn.Add(new XAttribute("Element", name.Value));
				foreach (var ia in isn.Attributes().Where(a => a.Name.Namespace == xic)) {
					sn.Add(new XAttribute(ia.Name.LocalName, ia.Value));
				}
				scene.Add(sn);
			}
		}

		public XElement CreateDirect(string filename, Dictionary<string, string> parameters) {
			XElement source;
			var file = InFilename(filename);
			source = XElement.Load(file, LoadOptions.PreserveWhitespace | LoadOptions.SetBaseUri | LoadOptions.SetLineInfo);
			if (source.Name == xic+"XamlImageConverter" || source.Name == sb+"SkinBuilder") {
				foreach (var key in validAttributes) parameters.Remove(key);
				return source;
			}

			XElement snapshot, scene;
			var res = new XElement(xic + "XamlImageConverter",
					new XAttribute(XNamespace.Xmlns + "xic", xic.NamespaceName),
					scene = new XElement(xic + "Scene",
						new XAttribute("OutputPath", Path.GetDirectoryName(filename.Replace("/","\\")).Replace("\\","/")),
						new XAttribute("Source", filename)
					)
				);
			if (parameters.Count > 0) {
				if (!parameters.Keys.Contains("xic")) {
					scene.Add(snapshot = new XElement(xic + "Snapshot"));
					ApplyParameters(null, snapshot, parameters);
				} else parameters.Remove("xic");
			}
			ParseXaml(source, scene);
			DynamicResult = true;
			if (parameters.Count > 0) return Dynamic;
			return res;
		}

		public XElement CreateDirect(string filename, XElement e, Dictionary<string, string> parameters) {
			XElement scene;
			if (e.Name != xic + "XamlImageConverter") {
				XNamespace mc = "http://schemas.openxmlformats.org/markup-compatibility/2006";
				var res = new XElement(xic + "XamlImageConverter",
						new XAttribute("xmlns", xamlns),
						new XAttribute(XNamespace.Xmlns + "x", xxamlns.NamespaceName),
						new XAttribute(XNamespace.Xmlns + "xic", xic.NamespaceName),
						new XAttribute(XNamespace.Xmlns + "mc", mc.NamespaceName),
						new XAttribute(XNamespace.Xmlns + "d", "http://schemas.microsoft.com/expression/blend/2008"),
						new XAttribute(mc + "Ignorable", "d"),
						scene = new XElement(xic + "Scene", new XElement(xic + "Xaml", e))
					);
				if (e.Name.NamespaceName == "") {
					e.Name = xamlns + e.Name.LocalName;
					e.SetAttributeValue(XNamespace.Xmlns + "x", xxamlns.NamespaceName);
					foreach (var child in e.Descendants()) {
						if (child.Name.NamespaceName == "") child.Name = xamlns + child.Name.LocalName;
					}
				}
				if (parameters.ContainsKey("Image") || parameters.ContainsKey("File") || parameters.ContainsKey("Filename") || parameters.ContainsKey("Type")) {
					var snapshot = new XElement(xic + "Snapshot");
					ApplyParameters(filename, snapshot, parameters);
					scene.Add(snapshot);
				}
				ParseXaml(e, scene);
				DynamicResult = true;
				if (parameters.Count > 0) return Dynamic;
				return res;
			} else {
				return e;
			}
		}

		public XElement CreateAxd(Dictionary<string, string> par) {
			var src = par["Source"];
			if (string.IsNullOrEmpty(src)) {
				DynamicResult = false;
				return Dynamic;
			}
			if (src.Trim()[0] == '#') src = (string)HttpContext.Current.Session[src];
			if (!src.Trim().StartsWith("<")) return CreateDirect(src, par);

			return CreateDirect("xic.axd", XElement.Parse(src, LoadOptions.PreserveWhitespace | LoadOptions.SetBaseUri | LoadOptions.SetLineInfo), par);
		}
	}

	public class Cache {

		public static string Path { get; set; }

		static XNamespace ns = XamlBuildCheck.ns;
		static XNamespace xic = XamlBuildCheck.xic;

		public static string File(string id, XElement xaml, string type = null, string path = null) {
			var e = xaml.DescendantsAndSelf(xic + id).FirstOrDefault();
			if (e != null) {
				if (e.Attribute(xic+"ImageType") != null) type = (string)e.Attribute(xic+"ImageType");
				if (e.Attribute(xic+"Cache") != null) path = (string)e.Attribute(xic+"Cache");
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
			public class Hash: Silversite.Services.Hash { }
#endif

#if Silversite
	[Configuration.Section(Name = "XamlImageConverter")]
	public class XamlImageConverterConfiguration : Silversite.Configuration.Section {
		[ConfigurationProperty("UseService", IsRequired = false, DefaultValue = false)]
		public bool UseService { get { return (bool)(this["UseService"] ?? true); } set { this["UseService"] = value; } }
		[ConfigurationProperty("Log", IsRequired = false, DefaultValue = true)]
		public bool Log { get { return (bool)(this["Log"] ?? true); } set { this["Log"] = value; } }
		[ConfigurationProperty("cache", IsRequired = false, DefaultValue = null)]
		public string Cache { get { return (string)this["cache"]; } set { this["cache"] = value; } }
		[ConfigurationProperty("separateDomain", IsRequired = false, DefaultValue = false)]
		public bool SeparateDomain { get { return (bool)(this["separateDomain"] ?? false); } set { this["separateDomain"] = value; } }
		[ConfigurationProperty("gcLevel", IsRequired = false, DefaultValue = 1)]
		public int GCLevel { get { return (bool)(this["gcLevel"] ?? 1); } set { this["gcLevel"] = value; } }
		[ConfigurationProperty("cores", IsRequired = false, DefaultValue = null)]
		public int? Cores { get { return (int?)(this["cores"]); } set { this["cores"] = value; } }
		[ConfigurationProperty("parallel", IsRequired = false, DefaultValue = false)]
		public bool Parallel { get { return (bool)(this["parallel"] ?? false); } set { this["parallel"] = value; } }
	}
#endif

	public class XamlImageHandler : System.Web.IHttpHandler, System.Web.SessionState.IReadOnlySessionState {

#if Silversite
		public static XamlImageConverterConfiguration Configuration = new XamlImageConverterConfiguration();
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
						handler = handlerInfo.New<IHttpHandler>();
#else
						var assemblyfile = context.Server.MapPath("~/Bin.Lazy/XamlImageConverter.dll");
						if (!File.Exists(assemblyfile)) assemblyfile = context.Server.MapPath("~/Bin/Lazy/XamlImageConverter.dll");
						if (!File.Exists(assemblyfile)) assemblyfile = context.Server.MapPath("~/Silversite/Bin/XamlImageConverter.dll");
						var aname = new AssemblyName("XamlImageConverter");
						aname.CodeBase = new Uri(assemblyfile).ToString();
						var a = Assembly.Load(aname);
						var type = a.GetType("XamlImageConverter.XamlImageHandler");
						handler = (System.Web.IHttpHandler)Activator.CreateInstance(type);
#endif
					} catch {
						context.Response.StatusCode = 500;
						context.ApplicationInstance.CompleteRequest();
					}
				}
#if Silversite
				context.Application.Lock();
				context.Application["XamlImageConverter.Configuration.UseService"] = Configuration.UseService;
				context.Application["XamlImageConverter.Configuration.Log"] = Configuration.Log;
				context.Application["XamlImageConverter.Configuration.Cache"] = Configuration.Cache;
				context.Application["XamlImageConverter.Configuration.SeparateDomain"] = Configuration.SeparateDomain;
				context.Application["XamlImageConverter.Configuration.GCLevel"] = Configuration.GCLevel;
				context.Application.UnLock();
#endif
				handler.ProcessRequest(context);
			} else {
				var filename = context.Request.AppRelativeCurrentExecutionFilePath;
				var image = context.Request.QueryString["Image"] ?? context.Request.QueryString["File"] ?? context.Request.QueryString["Filename"];
				var ext = Path.GetExtension(filename).ToLower();
				if (ext == ".xaml" || ext == ".psd" || ext == ".svg" || ext == ".svgz") {
					var exts = context.Request.QueryString.GetValues(null);
					if (exts != null && exts.Length > 0) {
						ext = exts[0];
						image = image ?? filename + "." + ext;
					}
				}
				var name = System.IO.Path.GetFileName(image);
				image = context.Server.MapPath(image);
				ext = System.IO.Path.GetExtension(image);
				if (!string.IsNullOrEmpty(ext)) ext = ext.Substring(1).ToLower();

				switch (ext) {
					case "bmp": context.Response.ContentType = "image/bmp"; break;
					case "png": context.Response.ContentType = "image/png"; break;
					case "jpg":
					case "jpeg":	context.Response.ContentType = "image/jpeg"; break;
					case "tif":
					case "tiff": context.Response.ContentType = "image/tiff"; break;
					case "gif": context.Response.ContentType = "image/gif"; break;
					case "wdp": context.Response.ContentType = "image/vnd.ms-photo"; break;
					case "pdf": 
						context.Response.AppendHeader("content-disposition", "attachment; filename=" + name);
						context.Response.ContentType = "application/pdf"; break;
					case "xps":
						context.Response.AppendHeader("content-disposition", "attachment; filename=" + name);
						context.Response.ContentType = "application/vnd.ms-xpsdocument"; break;
					case "eps":
					case "ps":
						context.Response.AppendHeader("content-disposition", "attachment; filename=" + name);
						context.Response.ContentType = "application/postscript"; break;
					case "psd":
						context.Response.AppendHeader("content-disposition", "attachment; filename=" + name);
						context.Response.ContentType = "image/photoshop"; break;
					case "svg": 
					case "svgz": context.Response.ContentType = "image/svg+xml"; break;
					case "xaml": context.Response.ContentType = "application/xaml+xml"; break;
					default: break;
				}

				if (System.IO.File.Exists(image)) context.Response.WriteFile(image);
				else {
					context.Response.StatusCode = 404;
				}
				context.ApplicationInstance.CompleteRequest();
			}
		}
	}
}