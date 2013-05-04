using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace XamlImageConverter.Web.UI {

	public class Compiler {

		static object compiler = null;

		public static void Compile(string xic) {
			var checker = new XamlBuildCheck() { Context = System.Web.HttpContext.Current };
			
			if (checker.NeedsBuilding(xic)) {
#if Silversite
						var handlerInfo = Services.Lazy.Types.Info("XamlImageConverter.Compiler");
						handlerInfo.Load();
						Compiler = handlerInfo.New();
#else
				var assemblyfile = HostingEnvironment.MapPath("~/Bin/Lazy/XamlImageConverter.dll");
				if (!System.IO.File.Exists(assemblyfile)) assemblyfile = HostingEnvironment.MapPath("~/Silversite/BinLazy/XamlImageConverter.dll");
				var aname = new System.Reflection.AssemblyName("XamlImageConverter");
				aname.CodeBase = new Uri(assemblyfile).ToString();
				var a = System.Reflection.Assembly.Load(aname);
				var type = a.GetType("XamlImageConverter.Compiler");
				compiler = Activator.CreateInstance(type);
#endif

				var ct = compiler.GetType();
				var projectPath = ct.GetProperty("ProjectPath");
				var libraryPath = ct.GetProperty("LibraryPath");
				var sourceFiles = ct.GetProperty("SourceFiles");
				projectPath.SetValue(compiler, HostingEnvironment.MapPath("~"));
				libraryPath.SetValue(compiler, HostingEnvironment.MapPath("~/bin"));
				var sources = new List<string>() { xic };
				sourceFiles.SetValue(compiler, sources);
				var compile = ct.GetMethod("Compile", new Type[0]);
				compile.Invoke(compiler, new object[0]);
			}
		}
	}
}
