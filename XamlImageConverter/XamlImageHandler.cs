using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Handlers;
using System.Diagnostics;
using System.Globalization;
using System.Configuration;

namespace XamlImageConverter {

	[Configuration.ConfigurationSection(Name = "XamlImageConverter")]
	public class HttpHandlerConfiguration: Configuration.ConfigurationSection {
		[ConfigurationProperty("useService", IsRequired = false, DefaultValue = false)]
		public bool UseSevice { get { return (bool)(this["useService"] ?? true); } set { this["useService"] = value; } }
		[ConfigurationProperty("log", IsRequired = false, DefaultValue = true)]
		public bool Log { get { return (bool)(this["log"] ?? true); } set { this["log"] = value; } }
		[ConfigurationProperty("cache", IsRequired = false, DefaultValue = null)]
		public string Cache { get { return (string)this["cache"]; } set { this["cache"] = value; } }
		[ConfigurationProperty("separateDomain", IsRequired = false, DefaultValue = false)]
		public bool SeparateDomain { get { return (bool)(this["separateDomain"] ?? false); } set { this["separateDomain"] = value; } }
		[ConfigurationProperty("gcLevel", IsRequired = false, DefaultValue = 1)]
		public int GCLevel { get { return (int)(this["gcLevel"] ?? 1); } set { this["gcLevel"] = value; } }
		[ConfigurationProperty("cores", IsRequired = false, DefaultValue = null)]
		public int? Cores { get { return (int?)(this["cores"]); } set { this["cores"] = value; } }
		[ConfigurationProperty("parallel", IsRequired = false, DefaultValue = false)]
		public bool Parallel { get { return (bool)(this["parallel"] ?? false); } set { this["parallel"] = value; } }
	}

	public class XamlImageHandler: System.Web.IHttpHandler, System.Web.SessionState.IReadOnlySessionState {

		public XamlImageHandler(): base() { }

		public static bool UseService { get; set; }
		public static bool Log { get; set; }
		public static string Cache { get; set; }
		public static bool SeparateDomain { get; set; }
		public int GCLevel { get; set; }
		public int? Cores { get; set; }
		public bool Parallel { get; set; }
		public static bool CustomConfig = false;

		public bool IsReusable {
			get { return true; }
		}

		void LoadConfig() {
			if (!CustomConfig) {
				var app = System.Web.HttpContext.Current.Application;
				app.Lock();
				var useService = (bool?)app["XamlImageConverter.Configuration.UseService"];
				var log = (bool?)app["XamlImageConverter.Configuration.Log"];
				var cache = (string)app["XamlImageConverter.Configuration.Cache"];
				var domain = (bool?)app["XamlImageConverter.Configuration.SeparateDomain"];
				var gclevel = (int?)app["XamlImageConverter.Configuration.GCLevel"];
				var cores = (int?)app["XamlImageConverter.Configuration.Cores"];
				var parallel = (bool?)app["XamlImageConverter.Configuration.Parallel"];
				app.UnLock();
				if (useService.HasValue || log.HasValue || cache != null || domain.HasValue) {
					if (useService.HasValue) UseService = useService.Value;
					if (log.HasValue) Log = log.Value;
					if (cache != null) Cache = cache;
					if (domain.HasValue) SeparateDomain = domain.Value;
					if (gclevel.HasValue) GCLevel = gclevel.Value;
					if (cores.HasValue) Cores = cores.Value;
					if (parallel.HasValue) Parallel = parallel.Value;
				} else {
					var c = new HttpHandlerConfiguration();
					UseService = c.UseSevice;
					Log = c.Log;
					Cache = c.Cache;
					SeparateDomain = c.SeparateDomain;
					GCLevel = c.GCLevel;
				}
				CustomConfig = true;
			}
		}

		public static Dictionary<string, object> Locks = new Dictionary<string, object>();


		public void Serve(System.Web.HttpContext context, string ext, string name, string image) {
			switch (ext) {
				case "bmp": context.Response.ContentType = "image/bmp"; break;
				case "png": context.Response.ContentType = "image/png"; break;
				case "jpg":
				case "jpeg": context.Response.ContentType = "image/jpeg"; break;
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
			else context.Response.StatusCode = 404;
			context.Response.End();
		}

		public void ProcessRequest(System.Web.HttpContext context) {
			try {
				var filename = context.Request.AppRelativeCurrentExecutionFilePath;
				var image = context.Request.QueryString["Image"] ?? context.Request.QueryString["File"] ?? context.Request.QueryString["Filename"];
				var direct = false;
				var par = new Dictionary<string, string>();
				var ext = System.IO.Path.GetExtension(filename).ToLower();
				var imgext = "";
				if (ext == ".xaml" || ext == ".psd" || ext == ".svg" || ext == ".svgz") {
					var exts = context.Request.QueryString.GetValues(null);
					if (exts != null && exts.Length > 0) {
						ext = exts[0];
						image = image ?? filename + "." + ext;
						par.Add("Type", ext);
					} else direct = image == null;
				} else if (ext != ".axd") direct = true;

				var name = System.IO.Path.GetFileName(image);
				image = context.Server.MapPath(image);
				ext = System.IO.Path.GetExtension(image).Substring(1).ToLower();

				if (!direct) {

					LoadConfig();
					var projpath = context.Server.MapPath("~");
					var libpath = context.Server.MapPath("~/bin");
					var skinpath = context.Server.MapPath(filename);
					string cultureid = context.Request.QueryString["Culture"];

					foreach (var key in context.Request.QueryString.AllKeys.Where(k => k != null)) {
						par.Add(key, context.Request.QueryString[key]);
					}

					var compiler = new Compiler(projpath, libpath);
					compiler.SourceFiles.Add(skinpath);
					compiler.RebuildAll = false;
					compiler.SeparateAppDomain = !UseService && SeparateDomain;
					compiler.UseService = UseService;
					compiler.Parameters = par;
					compiler.Context = context;
					compiler.Parallel = Parallel;
					compiler.Cores = Cores; 
					compiler.GCLevel = GCLevel;
					if (Log) compiler.Loggers.Add(new FileLogger());
					if (!string.IsNullOrEmpty(cultureid)) { compiler.Culture = CultureInfo.GetCultureInfo(cultureid); }

					compiler.Serve = () => {
						if (compiler.hash.HasValue) {
							image = System.IO.Path.ChangeExtension(image, "." + compiler.hash.Value.ToString("X") + System.IO.Path.GetExtension(image));
						}
						Serve(context, ext, name, image);
					};

					var lockpath = skinpath;
					if (lockpath.EndsWith("xic.axd")) lockpath = lockpath + image;
					lock (Locks) { if (!Locks.ContainsKey(skinpath)) Locks.Add(lockpath, new object()); }
					lock (Locks[lockpath]) {
						compiler.Compile();
					}

				} else {
					Serve(context, ext, name, image);
				}
			} catch (Exception ex) {
				context.Response.StatusCode = 500;
				context.Response.End();
			}
		}
	}
}
