using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Web;
using System.Threading;
using Awesomium.Core;

namespace XamlImageConverter {
	
	public static class Html2PDF {

		public static ManualResetEventSlim SavedSignal = new ManualResetEventSlim(true);
		public static IWebView View = null;
		public static WebSession Session = null;
		public static bool AssemblyResolverInitialized = false;
		public static int Instances = 0;
		public static IDisposable FileLock = null;
		public static object Lock = new object();

		public static void SaveAsync(string source, string localfile, string elementname, string width, string height, string dpi) {
			var path = Path.GetDirectoryName(localfile);
			//var source = ((Group.HtmlSource)s.Element).Source;
			string html = null;
			if (source.StartsWith("http://") || source.StartsWith("https://") || source.StartsWith("~")) {
				/*try {
					var ctx = HttpContext.Current;
					var app = ctx.Request.ApplicationPath;
					var root = ctx.Request.Url.Authority + "/" + app;
					var oldpath = ctx.Request.Url.AbsolutePath;
					path = null;
					if (source.StartsWith(root)) {
						path = source.Substring(root.Length);
					} else if (source.StartsWith("~")) {
						path = source.Substring(1);
					}
					if (path != null) {
						using (var w = new StringWriter()) {
							ctx.RewritePath(path);
							ctx.Server.Execute(path, w, false);
							ctx.RewritePath(oldpath);
							html = w.ToString();
						}
					}
				} catch {
				} finally {*/
				/*	if (html == null) {
						var web = new WebClient();
						web.Encoding = Encoding.UTF8;
						html = web.DownloadString(source);
					}
				//} */			
			}
			var binpath = Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath);

			//AppDomain.CurrentDomain.AppendPrivatePath("Lazy\\Awesomium");
			if (!AssemblyResolverInitialized) {
				int resolving = 0;
				AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => {
					try {
						resolving++;
						var apath = binpath;
						var aname = new System.Reflection.AssemblyName(args.Name);
						aname.CodeBase = Path.Combine(apath, args.Name + ".dll");
						return resolving > 1 ? null : System.Reflection.Assembly.Load(aname);
					} catch {
						return null;
					} finally {
						resolving--;
					}
				};
				AssemblyResolverInitialized = true;
			}

			if (Instances++ == 0) {
				WebCore.Initialize(new WebConfig() { LogPath = path, LogLevel = LogLevel.Verbose });
			}

			if (Session == null) Session = WebCore.CreateWebSession(new WebPreferences() {
				AcceptLanguage = Thread.CurrentThread.CurrentUICulture.Name.ToLower(),
				DefaultEncoding = "utf-8",
				EnableGPUAcceleration = false,
				FileAccessFromFileURL = true,
				UniversalAccessFromFileURL = true,
				WebAudio = false,
				WebGL = true,
				WebSecurity = false
			});
			if (View == null) View = WebCore.CreateWebView(1280, 1024, Session);
			var config = new PrintConfig();
			//s.GetSize(s.Element);
			config.PageSize = new AweRect(0, 0, (int)(double.Parse(width)+0.5), (int)(double.Parse(height)+0.5));
			config.SplitPagesIntoMultipleFiles = false;
			var Dpi = 600.0;
			double.TryParse(dpi, out Dpi);
			config.Dpi = Dpi;
			IDisposable fileLock = null;
			SavedSignal.Reset();
			View.PrintComplete += (sender, args) => {
				File.Move(args.Files [0], localfile);
				Environment.Exit(0);
			};
			View.PrintFailed += (sender, args) => {
				Environment.Exit(1);
			};
			var loaded = false;
			Action onloaded = () => {
				lock (Lock) {
					if (loaded) return;
					var part = !string.IsNullOrWhiteSpace(elementname);
					if (part) {
						View.ExecuteJavascript(@"
						var obj = top.document.getElementById('" + elementname + @"');
						var selection = window.getSelection();
						var range = document.createRange();
						range.setStartBefore(obj);
						range.setEndAfter(obj) ;
						selection.addRange(range);
					");
						config.PrintSelectionOnly = true;
					} else {
						config.PrintSelectionOnly = false;
					}
					//FileLock = s.Compiler.FileLock(tempFile);
					View.PrintToFile(path, config);
					loaded = true;
				}
			};
			View.LoadingFrameComplete += (sender, args) => { if (args.IsMainFrame) onloaded(); };
			View.LoadingFrameFailed += (sender, args) => { if (args.IsMainFrame) onloaded(); };

			View.Source = new Uri(source);
			while (View.IsLoading) { Thread.Sleep(100); WebCore.Update(); }
			Thread.Sleep(300);
			WebCore.Update();
			onloaded();
			while (View.IsPrinting) { Thread.Sleep(100); WebCore.Update(); }
		}

		public static void Main(string[] args) {
			try {
				SaveAsync(args[0], args[1], args[2], args[3], args[4], args[5]);
			} catch (Exception ex) {
				Environment.Exit(2);
			}
		}
	}

}
