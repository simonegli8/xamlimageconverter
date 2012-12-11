using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Windows;

namespace Tomers.WPF.Themes.Skins
{
	public sealed class AppDomainAssemblySkin : DirectAssemblySkin
	{
		public AppDomainAssemblySkin(string name, string assemblyPath) : base (name, assemblyPath)
		{
		}

		public AppDomainAssemblySkin(string name, AssemblyName fullName) : base(name, fullName)
		{
		}

		public AppDomainAssemblySkin(string name, string assemblyPath, string resourceName)
			: base(name, assemblyPath, resourceName)
		{
		}

		public AppDomainAssemblySkin(string name, AssemblyName fullName, string resourceName)
			: base(name, fullName, resourceName)
		{
		}

		protected override ISkinBamlResolver PreLoadResources()
		{
			_assemblySkinDomain = AppDomain.CreateDomain(Name);
			ISkinBamlResolver skinResolver = (ISkinBamlResolver)_assemblySkinDomain.CreateInstanceAndUnwrap(
					Assembly.GetExecutingAssembly().FullName,
					typeof(SkinBamlResolver).FullName);
			return skinResolver;			
		}

		protected override void PostLoadResources()
		{
			if (_assemblySkinDomain != null)
			{
				AppDomain.Unload(_assemblySkinDomain);
				_assemblySkinDomain = null;
			}
		}

		private AppDomain _assemblySkinDomain;
	}
}