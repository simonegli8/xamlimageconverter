using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using XamlImageConverter;

namespace XamlImageConverter {

	public class Logger: MarshalByRefObject, ILogger {

		public Logger(TaskLoggingHelper log) { Log = log; }

		public TaskLoggingHelper Log { get; set; }
		
		public void Message(string path, string message, string errorCode, TextSpan span, Severity severity) {
			if (Log != null) {
				switch(severity) {
				case Severity.Error: Log.LogError(string.Empty, errorCode, string.Empty, path, span.Start.Line, span.Start.Column, span.End.Line, span.End.Column, message); break;
				case Severity.Warning: Log.LogWarning(string.Empty, errorCode, string.Empty, path, span.Start.Line, span.Start.Column, span.End.Line, span.End.Column, message); break;
				case Severity.Message: Log.LogMessage(MessageImportance.High, message); break;
				case Severity.Note: Log.LogMessage(MessageImportance.High, message); break;
				}
			}
		}
		public void Flush() { }
	}

}
