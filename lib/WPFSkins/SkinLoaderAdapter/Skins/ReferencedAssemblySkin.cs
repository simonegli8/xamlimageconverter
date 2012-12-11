using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Windows;

namespace Tomers.WPF.Themes.Skins
{
	public sealed class ReferencedAssemblySkin : Skin
	{
		public ReferencedAssemblySkin(string name, Uri resourceUri) : base(name)
		{
			this._resourceUri = resourceUri;
		}

		protected override void LoadResources()
		{
			ResourceDictionary resource = (ResourceDictionary)Application.LoadComponent(_resourceUri);
			Resources.Add(resource);
		}

		private readonly Uri _resourceUri;
	}
}
