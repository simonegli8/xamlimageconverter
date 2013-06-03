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
	
	public class Html2PDF: Snapshot.Html2PDFConverter {

		public ManualResetEventSlim SavedSignal = new ManualResetEventSlim(true);
		public IWebView View = null;
		public WebSession Session = null;
		public static bool AssemblyResolverInitialized = false;
		public int Instances = 0;
		public IDisposable FileLock = null;

		public void SaveAsnc(Snapshot s) {
			string path;
			var source = ((Group.HtmlSource)s.Element).Source;
			string html = null;
			if (source.StartsWith("http://") || source.StartsWith("https://")) {
				try {
					var ctx = HttpContext.Current;
					var app = ctx.Request.ApplicationPath;
					var root = ctx.Request.Url.Authority + "/" + app;
					if (source.StartsWith(root)) {
						var oldpath = ctx.Request.Url.AbsolutePath;
						path = source.Substring(root.Length);
						using (var w = new StringWriter()) {
							ctx.RewritePath(path);
							ctx.Server.Execute(path, w, false);
							ctx.RewritePath(oldpath);
							html = w.ToString();
						}
					}
 				} catch { }
				/*if (html == null) {
					var web = new WebClient();
					web.Encoding = Encoding.UTF8;
					html = web.DownloadString(source);
				}*/
			} else {
				html = File.ReadAllText(s.Compiler.MapPath(source));
			}

			//AppDomain.CurrentDomain.AppendPrivatePath("Lazy\\Awesomium");
			if (!AssemblyResolverInitialized) {
				int resolving = 0;
				AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => {
					try {
						resolving++;
						var apath = s.Compiler.BinPath("Lazy\\Awesomium\\");
						var aname = new System.Reflection.AssemblyName(args.Name);
						aname.CodeBase = apath + args.Name + ".dll";
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
				WebCore.Initialize(new WebConfig() { LogPath = s.Compiler.SkinPath, LogLevel = LogLevel.Verbose });
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
			var size = s.GetSize(s.Element);
			config.PageSize = new AweRect(0, 0, (int)size.Width, (int)size.Height);
			config.SplitPagesIntoMultipleFiles = false;
			config.Dpi = s.Dpi ?? 600;
			path = Path.GetDirectoryName(s.LocalFilename);
			var tempFile = path + "/doc_1.pdf";
			IDisposable fileLock = null;
			SavedSignal.Reset();
			View.PrintComplete += (sender2, args2) => {
				File.Move(tempFile, s.LocalFilename);
				FileLock.Dispose();
				FileLock = null;
				SavedSignal.Set();
				s.Errors.Message("{0} created.", Path.GetFileName(s.LocalFilename));
				s.ImageCreated();
			};
			View.PrintFailed += (sender2, args2) => {
				s.Errors.Error("Failed converting html to pdf.", "61", s.XElement);
				FileLock.Dispose();
				FileLock = null;
				SavedSignal.Set();
			};
			var loaded = false;
			View.LoadingFrameComplete += (sender, args) => {
				if (args.IsMainFrame) {
					var part = !string.IsNullOrEmpty(s.ElementName);
					if (part) {
						View.ExecuteJavascript(@"
						var obj = top.document.getElementById('" + s.ElementName + @"');
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
					FileLock = s.Compiler.FileLock(tempFile);
					View.PrintToFile(path, config);
					loaded = true;
				}
			};

			if (html != null) View.LoadHTML(html);
			else View.Source = new Uri(source);
			while (!loaded) { Thread.Sleep(100); WebCore.Update(); }
		}

		public void AwaitSave() {
			while (!SavedSignal.Wait(100)) WebCore.Update();

			if (FileLock != null) FileLock.Dispose();
			if (View != null) View.Dispose();
			if (Session != null) Session.Dispose();
			if (--Instances == 0) WebCore.Shutdown();
		}

	}

}
