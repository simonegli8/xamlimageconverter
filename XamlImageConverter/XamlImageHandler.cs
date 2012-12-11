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

		[ConfigurationProperty("UseService", IsRequired = false, DefaultValue = false)]
		public bool UseSevice { get { return (bool)(this["UseService"] ?? true); } set { this["UseService"] = value; } }

		[ConfigurationProperty("Log", IsRequired = false, DefaultValue = true)]
		public bool Log { get { return (bool)(this["Log"] ?? true); } set { this["Log"] = value; } }

		[ConfigurationProperty("Cache", IsRequired = false, DefaultValue = null)]
		public string Cache { get { return (string)this["Cache"]; } set { this["Cache"] = value; } }

	}

	public class XamlImageHandler: System.Web.IHttpHandler {

		public XamlImageHandler(): base() { }

		public static bool UseService { get; set; }
		public static bool Log { get; set; }
		public static string Cache { get; set; }
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
				app.UnLock();
				if (useService.HasValue || log.HasValue || cache != null) {
					if (useService.HasValue) UseService = useService.Value;
					if (log.HasValue) Log = log.Value;
					if (cache != null) Cache = cache; 
				} else {
					var c = new HttpHandlerConfiguration();
					UseService = c.UseSevice;
					Log = c.Log;
					Cache = c.Cache;
				}
				CustomConfig = true;
			}
		}

		public void ProcessRequest(System.Web.HttpContext context) {
			LoadConfig();
			var filename = context.Request.AppRelativeCurrentExecutionFilePath;
			var projpath = context.Server.MapPath("~");
			var libpath = context.Server.MapPath("~/bin");
			var skinpath = context.Server.MapPath(filename);
			string cultureid = context.Request.QueryString["Culture"];

			var par = new Dictionary<string, string>();
			foreach (var key in context.Request.QueryString.AllKeys) {
				if (key != "Culture" && key != "Image") {
					par.Add(key, context.Request.QueryString[key]);
				}
			}

			var compiler = new Compiler(projpath, libpath);
			compiler.SourceFiles.Add(skinpath);
			compiler.RebuildAll = false;
			compiler.SeparateAppDomain = !UseService;
			compiler.UseService = UseService;
			compiler.Parameters = par;
			if (Log) compiler.Loggers.Add(new FileLogger());
			if (!string.IsNullOrEmpty(cultureid)) { compiler.Culture = CultureInfo.GetCultureInfo(cultureid); }
			compiler.Compile();

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
