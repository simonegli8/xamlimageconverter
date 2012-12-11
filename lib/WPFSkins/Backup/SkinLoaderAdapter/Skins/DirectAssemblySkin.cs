using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Windows;

namespace Tomers.WPF.Themes.Skins
{
	public class DirectAssemblySkin : Skin
	{
		public DirectAssemblySkin(string name, string assemblyPath) : base (name)
		{
			if (string.IsNullOrEmpty(assemblyPath))
			{
				throw new ArgumentException("Invalid assembly path", "assemblyPath");
			}
			this._fullName = AssemblyName.GetAssemblyName(assemblyPath);
		}

		public DirectAssemblySkin(string name, AssemblyName fullName) : base (name)
		{
			if (fullName == null)
			{
				throw new ArgumentException("Invalid assembly name", "fullName");
			}
			this._fullName = fullName;
		}

		public DirectAssemblySkin(string name, string assemblyPath, string resourceName) : base(name)
		{
			if (string.IsNullOrEmpty(resourceName))
			{
				throw new ArgumentException("Invalid resource name", "assemblyPath");
			}
			this._fullName		= AssemblyName.GetAssemblyName(assemblyPath);
			this._resourceName	= resourceName;
			FixResourceName();
		}		

		public DirectAssemblySkin(string name, AssemblyName fullName, string resourceName) : base(name)
		{
			if (string.IsNullOrEmpty(resourceName))
			{
				throw new ArgumentException("Invalid resource name", "assemblyPath");
			}
			this._fullName		= fullName;
			this._resourceName	= resourceName;
			FixResourceName();
		}

		protected AssemblyName FullName
		{
			get { return _fullName; }
		}

		protected sealed override void LoadResources()
		{
			ISkinBamlResolver skinResolver = PreLoadResources();
			try
			{
				List<Stream> skinBamlStreams = skinResolver.GetSkinBamlStreams(_fullName, _resourceName);
				foreach (Stream resourceStream in skinBamlStreams)
				{
					ResourceDictionary skinResource = BamlHelper.LoadBaml<ResourceDictionary>(resourceStream);
					if (skinResource != null)
					{
						Resources.Add(skinResource);
					}
				}
			}
			finally
			{
				PostLoadResources();
			}
		}

		protected virtual ISkinBamlResolver PreLoadResources() { return new SkinBamlResolver(); }
		protected virtual void PostLoadResources() { }

		private void FixResourceName()
		{
			_resourceName = _resourceName.ToLower().Replace(".xaml", ".baml");
		}

		private readonly AssemblyName _fullName;
		private string _resourceName;		
	}
}
