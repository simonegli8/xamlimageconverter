using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.Reflection;
using System.Diagnostics.Contracts;

namespace XamlImageConverter {
	
	[Serializable]
	public enum Severity { Message, Warning, Error, Note, Status };

	[Serializable]
	public struct TextPos {
		public int Line, Column;
	}

	[Serializable]
	public struct TextSpan {
		public TextPos Start, End;

		public TextSpan(XObject x) {
			if (x == null) {
				Start = new TextPos { Line = 0, Column = 0 };
				End = new TextPos { Line = 0, Column = 0 };
			} else {
				Start = new TextPos { Line = ((IXmlLineInfo)x).LineNumber, Column = ((IXmlLineInfo)x).LinePosition };
				if (x is XElement) {
					var e = x as XElement;
					if (e.LastNode != null && e.LastNode.NodeType == XmlNodeType.EndElement) {
						End = new TextPos { Line = ((IXmlLineInfo)e.LastNode).LineNumber, Column = ((IXmlLineInfo)e.LastNode).LinePosition };
					} else {
						End = new TextPos { Line = ((IXmlLineInfo)e).LineNumber, Column = ((IXmlLineInfo)e).LinePosition + e.Name.NamespaceName.Length + e.Name.LocalName.Length };
					}
				} else if (x is XAttribute) {
					var a = x as XAttribute;
					End = new TextPos { Line = Start.Line, Column = Start.Column + a.Name.LocalName.Length + a.Value.Length + 1 };
				} else {
					End = new TextPos { Line = Start.Line, Column = Start.Column + 1 };
				}
			}
		}

	}

	public interface ILogger {
		void Message(string path, string message, string errorNumber, TextSpan span, Severity severity);
		void Flush(string path);
		void Clear(string path);
	}

	public class ConsoleLogger: MarshalByRefObject, ILogger {
		
		public virtual void Message(string path, string message, string errorCode, TextSpan span, Severity severity) {
			switch (severity) {
				case Severity.Error: Console.Write("  Error {0} ({1},{2}): ", errorCode, span.Start.Line, span.Start.Column); break;
				case Severity.Warning: Console.Write("  Warning {0} ({1},{2}): ", errorCode, span.Start.Line, span.Start.Column); break;
				case Severity.Status: break;
				default: Console.Write("   "); break;
			}
			Console.WriteLine(message);
		}
		public void Flush(string path) { }
		public void Clear(string path) { }
	}

	public class FileLogger: MarshalByRefObject, ILogger, IDisposable {
		string lastPath = string.Empty;
		Dictionary<string, StringBuilder> texts = new Dictionary<string, StringBuilder>();

		public virtual void Message(string path, string message, string errorCode, TextSpan span, Severity severity) {
			lock (texts) {
				if (!texts.ContainsKey(path)) texts.Add(path, new StringBuilder());
				var text = texts[path];
				switch (severity) {
				case Severity.Error: text.Append(string.Format("  Error {0} ({1},{2}): ", errorCode, span.Start.Line, span.Start.Column)); break;
				case Severity.Status: break;
				default: text.Append("   "); break;
				}
				text.AppendLine(message);
			}
		}

		public void Flush(string path) {
			lock (texts) {
				if (texts.ContainsKey(path)) {
					var text = texts[path];
					var str = text.ToString();
					var logpath = path + ".log";
					try {
						if (!string.IsNullOrEmpty(str)) System.IO.File.WriteAllText(logpath, str.Replace(Environment.NewLine, "\n").Replace("\n", Environment.NewLine));
						else if (System.IO.File.Exists(logpath)) System.IO.File.Delete(logpath);
					} catch { }
					texts.Remove(path);
				}
			}
		}

		public void Clear(string path) {
			lock (texts) {
				if (texts.ContainsKey(path)) texts.Remove(path);
			}
		}

		public void Dispose() {
			foreach (var path in texts.Keys.ToList()) Flush(path);
		}
	}

	[Serializable]
	public class Errors: MarshalByRefObject {
		HashSet<string> Paths = new HashSet<string>(); 

		public bool HasErrors = false;

		private List<ILogger> logger = new List<ILogger> { new ConsoleLogger() };
		public List<ILogger> Loggers { get {return logger; } set { logger = value; } }

		public string Path { get; set; }

		private void Write(string path, string message, string errorCode, TextSpan span, Severity severity) {
			if (path == null) return;
			lock (this) {
				if (!Paths.Contains(path)) Paths.Add(path);
				foreach (var logger in Loggers) {
					try {
						logger.Message(path, message, errorCode, span, severity);
					} catch {
					}
				}
			}
		}

		public Errors Clone(string path) {
			return new Errors { Loggers = this.Loggers, Path = path };
		}

		public void LocalMessage(string path, string message) { Write(path, message, string.Empty, new TextSpan(), Severity.Message); }
		public void LocalMessage(string path, string message, params object[] args) { Write(path, string.Format(message, args), string.Empty, new TextSpan(), Severity.Message); }
		public void Message(string message) { Write(Path, message, string.Empty, new TextSpan(), Severity.Message); }
		public void Message(string message, params object[] args) { Write(Path, string.Format(message, args), string.Empty, new TextSpan(), Severity.Message); }
		public void Note(string message) { Write(Path, message, string.Empty, new TextSpan(), Severity.Note); }
		public void Note(string message, params object[] args) { Write(Path, string.Format(message, args), string.Empty, new TextSpan(), Severity.Note); }
		public void Error(string message, string errorCode, TextSpan span) { Write(Path, message, errorCode, span, Severity.Error); }
		public void Warning(string message, string errorCode, TextSpan span) { Write(Path, message, errorCode, span, Severity.Warning); }
		public void Error(string message, string errorCode, XObject xobj) { HasErrors = true; Write(Path, message, errorCode, new TextSpan(xobj), Severity.Error); }
		public void Warning(string message, string errorCode, XObject xobj) { Write(Path, message, errorCode, new TextSpan(xobj), Severity.Warning); }
		public void Status(string message) { Write(Path, message, string.Empty, new TextSpan(), Severity.Status); }
		public void Status(string message, params object[] args) { Write(Path, string.Format(message, args), string.Empty, new TextSpan(), Severity.Status); }
		public void Clear() {
			foreach (var log in Loggers) log.Clear(Path);
		}
		public void Flush() {
			foreach (var log in Loggers) log.Flush(Path);
		}
	}
}
