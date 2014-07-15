using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Web;
using System.Threading;

namespace XamlImageConverter {
	
	public class Html2PDF: Snapshot.Html2PDFConverter {

		AutoResetEvent Signal = new AutoResetEvent(false);
		IDisposable filelock;
		Snapshot Snapshot;
		System.Diagnostics.Process process;
		string file;

		public void SaveAsync(Snapshot s) {
			Snapshot = s;
			var source = ((Group.HtmlSource)s.Element).Source;
			if (!source.StartsWith("http://") && !source.StartsWith("https://") && !source.StartsWith("~")) source = s.Compiler.MapPath(source);
			else if (source.StartsWith("~")) {
				try {
					var ctx = HttpContext.Current;
					var app = ctx.Request.ApplicationPath;
					var root = ctx.Request.Url.Authority + "/" + app;
					source = root + source.Substring(1);
				} catch { }
			}
	
			var exe = s.Compiler.BinPath("Lazy\\Awesomium\\html2pdf.exe");
			file = s.LocalFilename;
			var path = Path.GetDirectoryName(file);

			var size = s.GetSize(s.Element);
			var args = " \"" + source + "\" \"" + file +  "\" \"" + s.ElementName +  "\" " + size.Width.ToString() + " " + size.Height.ToString() + " " + (s.Dpi ?? 600).ToString();
			process = s.NewProcess(exe, args);
			filelock = s.FileLock(file);

			process.Exited +=(sender, args2) => {
				Signal.Set();
			};
			s.Processes.Add(process);
			process.Start();
		}

		public void AwaitSave() {
			Signal.WaitOne();
			try {
				if (process.ExitCode == 0) {
					Snapshot.Errors.Message("Created {0} ({1} MB RAM used)", Path.GetFileName(file), System.Environment.WorkingSet / (1024 * 1024));
				} else {
					Snapshot.Errors.Error("Failed converting html to pdf.", "61", Snapshot.XElement);
				}
			} finally {
				filelock.Dispose();
				Snapshot.ImageCreated();
				Snapshot.ExitProcess(process);
			}
		}
	}

}
