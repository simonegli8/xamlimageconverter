using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.Reflection;

namespace XamlImageConverter {
	
	[Serializable]
	public enum Severity { Message, Warning, Error, Note };

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
	}

	public class ConsoleLogger: MarshalByRefObject, ILogger {
		
		public virtual void Message(string path, string message, string errorCode, TextSpan span, Severity severity) {
			switch (severity) {
			case Severity.Error: Console.Write("  Error {0} ({1},{2}): ", errorCode, span.Start.Line, span.Start.Column); break;
			case Severity.Warning: Console.Write("  Warning {0} ({1},{2}): ", errorCode, span.Start.Line, span.Start.Column); break;
			default: break;
			}
			Console.WriteLine(message);
		}
	}

	public class FileLogger: MarshalByRefObject, ILogger, IDisposable {
		string lastPath = string.Empty;
		StringBuilder text = new StringBuilder();

		public virtual void Message(string path, string message, string errorCode, TextSpan span, Severity severity) {
			if (path != lastPath) Flush(path);
			switch (severity) {
			case Severity.Error: text.Append(string.Format("  Error {0} ({1},{2}): ", errorCode, span.Start.Line, span.Start.Column)); break;
			case Severity.Warning: text.Append(string.Format("  Warning {0} ({1},{2}): ", errorCode, span.Start.Line, span.Start.Column)); break;
			default: break;
			}
			text.AppendLine("   " + message);
		}

		private void Flush(string newpath) {
			if (!StringUtil.IsNullOrWhiteSpace(lastPath)) {
				var str = text.ToString();
				var logpath = lastPath + ".log";
				if (!string.IsNullOrEmpty(lastPath)) {
					try {
						if (str != null && str.Trim() != string.Empty) System.IO.File.WriteAllText(logpath, str);
						else if (System.IO.File.Exists(logpath)) System.IO.File.Delete(logpath);
					} catch { }
				}
				text = new StringBuilder();
			}
			lastPath = newpath;
		}

		public void Dispose() {
			Flush(string.Empty);
		}
	}

	[Serializable]
	public class Errors: MarshalByRefObject {
		string lastPath = string.Empty; 

		public bool HasErrors = false;

		private List<ILogger> logger = new List<ILogger> { new ConsoleLogger() };
		public List<ILogger> Loggers { get {return logger; } set { logger = value; } }

		public string Path { get; set; }

		private void Write(string message, string errorCode, TextSpan span, Severity severity) {
			if (Path != lastPath && !StringUtil.IsNullOrWhiteSpace(Path)) {
				lastPath = Path;
				Write(System.IO.Path.GetFileName(Path) + ":", "", new TextSpan(), Severity.Message);
			} else {
				lastPath = Path;
			}
			foreach (var logger in Loggers) logger.Message(Path, message, errorCode, span, severity);
		}

		public void Message(string message) { Write(message, string.Empty, new TextSpan(), Severity.Message); }
		public void Message(string message, params object[] args) { Write(string.Format(message, args), string.Empty, new TextSpan(), Severity.Message); }
		public void Note(string message) { Write(message, string.Empty, new TextSpan(), Severity.Note); }
		public void Note(string message, params object[] args) { Write(string.Format(message, args), string.Empty, new TextSpan(), Severity.Note); }
		public void Error(string message, string errorCode, TextSpan span) { Write(message, errorCode, span, Severity.Error); }
		public void Warning(string message, string errorCode, TextSpan span) { Write(message, errorCode, span, Severity.Warning); }
		public void Error(string message, string errorCode, XObject xobj) { HasErrors = true; Write(message, errorCode, new TextSpan(xobj), Severity.Error); }
		public void Warning(string message, string errorCode, XObject xobj) { Write(message, errorCode, new TextSpan(xobj), Severity.Warning); }
	}
}
