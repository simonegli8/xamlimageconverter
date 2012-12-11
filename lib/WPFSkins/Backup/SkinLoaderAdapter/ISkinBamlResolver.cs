using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace Tomers.WPF.Themes
{
	public interface ISkinBamlResolver
	{
		List<Stream> GetSkinBamlStreams(AssemblyName skinAssemblyName);
		List<Stream> GetSkinBamlStreams(AssemblyName skinAssemblyName, string resourceName);
	}
}
