using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XamlImageConverter {

	public class Web {

		public static string Combine(string path1, string path2) {
			string path = "";
			int slash;

			if (path2.StartsWith("/..")) path2 = path2.Substring(1);

			while (path2.StartsWith("../")) { // resolve relative paths.
				if (path1.EndsWith("/")) {
					slash = path1.LastIndexOf('/', path1.Length-1);
					if (slash <= 0) slash = 0;
					path1 = path1.Substring(0, slash);
				} else {
					slash = path1.LastIndexOf('/', path1.Length-1);
					if (slash <= 0) slash = 0;
					slash = path1.LastIndexOf('/', slash-1);
					if (slash <= 0) slash = 0;
					path1 = path1.Substring(0, slash);
				}
				path2 = path2.Substring(3);
			}

			if (path2.StartsWith("~")) path2 = path2.Substring(1);
			if (path1.EndsWith("/")) {
				if (path2.StartsWith("/")) path2 = path2.Substring(1);
				path = path1 + path2;
			} else if (path2.StartsWith("/")) {
				path = path1 + path2;
			} else {
				path = path1 + "/" + path2;
			}
			return path;
		}


		public static string Absolute(string path) {
			if (path.StartsWith("~")) {
				path = Normalize(path).Substring(1);
				var app = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;
				if (app.Length > 1) path = Combine(app, path);
				return path;
			} else {
				if (!path.StartsWith("/")) return "/" + path;
				return path;
			}
		}

		public static string Normalize(string path) {
			if (path == null) return "~";
			if (path.EndsWith("/")) path = path.Substring(0, path.Length - 1);
			if (path.StartsWith("/")) {
				var app = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;
				if (path.StartsWith(app, StringComparison.OrdinalIgnoreCase)) {
					path = path.Substring(app.Length);
					if (path.StartsWith("/")) return "~" + path;
					else return "~/" + path;
				} else throw new NotSupportedException("Absolute path with invalid ApplicationPath.");
			} else if (path.StartsWith("~")) return path;
			else return path;
		}

		public static string Home {
			get {
				var context = System.Web.HttpContext.Current;
				string home = context.Request.Url.AbsoluteUri, appl = context.Request.ApplicationPath;
				if (appl.StartsWith("~")) appl = appl.Substring(1);
				if (appl != "/") {
					home = home.Substring(0, home.IndexOf(appl)) + appl;
				} else {
					home = context.Request.Url.Scheme + "://" + context.Request.Url.Authority;
				}
				if (home.EndsWith("/")) home = home.Substring(0, home.Length-1);
				return home;
			}
		}
		public static bool IsLocal(string url) {
			return !url.Contains(":") || url.StartsWith(Home);
		}

		public static string Execute(string path) {
			using (var w = new System.IO.StringWriter()) {
				var ctx = System.Web.HttpContext.Current;
				path = Absolute(path.Replace(Home, "~"));
				var oldpath = ctx.Request.Url.AbsolutePath;
				ctx.RewritePath(path);
				ctx.Server.Execute(path, w, false);
				ctx.RewritePath(oldpath);
				return w.ToString();
			}
		}

		public static string Html(string url) {
			if (IsLocal(url)) {
				return Execute(url);
			} else {
				var web = new System.Net.WebClient();
				return web.DownloadString(url);
			}
		}
	}
}
