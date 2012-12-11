using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Tomers.WPF.Themes.Skins
{
	public abstract class Skin
	{
		private Skin() { }

		protected Skin(string name)
		{
			this._name		= name;
		}

		public string Name
		{
			get { return _name; }
		}

		protected List<ResourceDictionary> Resources
		{
			get { return _resources; }
		}

		public virtual void Load()
		{
			if (Resources.Count != 0)
			{
				// Already loaded
				return;
			}
			LoadResources();
			foreach (ResourceDictionary skinResource in Resources)
			{
				Application.Current.Resources.MergedDictionaries.Add(skinResource);
			}
		}

		public virtual void Unload()
		{
			foreach (ResourceDictionary skinResource in Resources)
			{
				Application.Current.Resources.MergedDictionaries.Remove(skinResource);
			}
			Resources.Clear();
		}

		protected abstract void LoadResources();

		private sealed class NullSkin : Skin
		{
			protected override void LoadResources() { }
		}

		public static readonly Skin Null = new NullSkin();
		private readonly List<ResourceDictionary> _resources = new List<ResourceDictionary>();
		private readonly string _name;
	}
}
