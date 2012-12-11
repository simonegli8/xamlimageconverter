using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Windows.Markup;

namespace Tomers.WPF.Themes
{
	public static class BamlHelper
	{
		static BamlHelper()
		{
			Type type = typeof(XamlReader);
			// Hope that Microsoft will not change this in the future, or at least provide an official way to load baml
			LoadBamlMethod = type.GetMethod("LoadBaml", BindingFlags.NonPublic | BindingFlags.Static);
		}

		public static TRoot LoadBaml<TRoot>(Stream stream)
		{
			ParserContext parserContext = new ParserContext();
			object[] parameters = new object[] { stream, parserContext, null, false };
			object bamlRoot = LoadBamlMethod.Invoke(null, parameters);
			return (TRoot)bamlRoot;
		}

		private static readonly MethodInfo LoadBamlMethod;
	}
}
