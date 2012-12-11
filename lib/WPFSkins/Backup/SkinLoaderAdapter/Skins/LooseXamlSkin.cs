using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Diagnostics;

namespace Tomers.WPF.Themes.Skins
{
	public sealed class LooseXamlSkin : Skin
	{
		public LooseXamlSkin(string name, Uri source) : base (name)
		{
			this._sources = new List<Uri>();
			this._sources.Add(source);
		}

		public LooseXamlSkin(string name, IList<Uri> sources) : base (name)
		{
			this._sources = new List<Uri>(sources);
		}

		protected override void LoadResources()
		{
			foreach (Uri uri in _sources)
			{
				ResourceDictionary skinDictionary = new ResourceDictionary();
				try
				{
					skinDictionary.Source = uri;
				}
				catch (Exception ex)
				{
					Debug.WriteLine("Change error: " + ex.ToString());
					throw;
				}
				Resources.Add(skinDictionary);
			}
		}

		private readonly List<Uri> _sources;
	}
}
