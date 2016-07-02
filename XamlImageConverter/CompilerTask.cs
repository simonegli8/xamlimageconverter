using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Linq;
using System.Threading;
using XamlImageConverter;



namespace XamlImageConverter.MSBuild {
	/////////////////////////////////////////////////////////////////////////////
	// My MSBuild Task
	public class XamlImageConverter : Task {

		public string Mode { get; set; } = "Both";

		private ICompiler compiler = null;

		/// <summary>
		/// Constructor. This is the constructor that will be used
		/// when the task run.
		/// </summary>
		public XamlImageConverter() {
		}

		/// <summary>
		/// Constructor. The goal of this constructor is to make
		/// it easy to test the task.
		/// </summary>
		public XamlImageConverter(ICompiler compilerToUse) {
			compiler = compilerToUse;
		}

		private string[] sourceFiles;
		/// <summary>
		/// List of Python source files that should be compiled into the assembly
		/// </summary>
		[Required()]
		public string[] SourceFiles
		{
			get { return sourceFiles; }
			set { sourceFiles = value; }
		}

		private string projectPath = null;
		/// <summary>
		/// This should be set to $(MSBuildProjectDirectory)
		/// </summary>
		public string ProjectPath
		{
			get { return projectPath; }
			set { projectPath = value; }
		}


		public string Parameters { get; set; }

		public string LibraryPath { get; set; }

		private bool parallel = true;
		public bool Parallel { get { return parallel; } set { parallel = value; } }
		/// <summary>
		/// Main entry point for the task
		/// </summary>
		/// <returns></returns>
		public override bool Execute() {

			// Create the compiler if it does not already exist
			if (compiler == null) {
				compiler = new Compiler();
			}

#if DEBUG
			//System.Diagnostics.Debugger.Launch();
#endif
			compiler.SourceFiles = SourceFiles.ToList();
			compiler.ProjectPath = ProjectPath;
			compiler.LibraryPath = LibraryPath;
			Compiler.Modes mode;
			if (!Enum.TryParse<Compiler.Modes>(Mode, out mode))
				throw new ArgumentOutOfRangeException("Valid values for Mode in XamlImageConverter are Loose, Compiled or Both");
			else compiler.Mode = mode;
			if (!string.IsNullOrEmpty(Parameters)) {
				var pars = Parameters.Split(';').Select(p => p.Split('=').Select(t => t.Trim()));
				foreach (var par in pars) compiler.Parameters.Add(par.First(), par.Last());
			}
			compiler.Loggers.Add(new Logger(Log));
			compiler.Loggers.Add(new FileLogger());
			compiler.SeparateAppDomain = true;
			compiler.Parallel = Parallel;
			compiler.Compile();

			return !Log.HasLoggedErrors;
		}

	}
}