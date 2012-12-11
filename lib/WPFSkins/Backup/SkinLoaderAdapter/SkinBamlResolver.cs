using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Resources;

namespace Tomers.WPF.Themes
{
	public class SkinBamlResolver : MarshalByRefObject, ISkinBamlResolver
	{
		#region ISkinResolver Members

		List<Stream> ISkinBamlResolver.GetSkinBamlStreams(AssemblyName skinAssemblyName)
		{
			ISkinBamlResolver resolver = this as ISkinBamlResolver;
			return resolver.GetSkinBamlStreams(skinAssemblyName, string.Empty);
		}

		List<Stream> ISkinBamlResolver.GetSkinBamlStreams(AssemblyName skinAssemblyName, string bamlResourceName)
		{
			List<Stream> skinBamlStreams = new List<Stream>();
			Assembly skinAssembly = Assembly.Load(skinAssemblyName);
			string[] resourcesNames = skinAssembly.GetManifestResourceNames();
			foreach (string resourceName in resourcesNames)
			{
				ManifestResourceInfo resourceInfo = skinAssembly.GetManifestResourceInfo(resourceName);
				if (resourceInfo.ResourceLocation != ResourceLocation.ContainedInAnotherAssembly)
				{
					Stream resourceStream = skinAssembly.GetManifestResourceStream(resourceName);
					using (ResourceReader resourceReader = new ResourceReader(resourceStream))
					{
						foreach (DictionaryEntry entry in resourceReader)
						{
							if (IsRelevantResource(entry, bamlResourceName))
							{
								skinBamlStreams.Add(entry.Value as Stream);
							}
							
						}
					}
				}
			}
			return skinBamlStreams;
		}

		private bool IsRelevantResource(DictionaryEntry entry, string resourceName)
		{
			string entryName = entry.Key as string;
			string extension = Path.GetExtension(entryName);
			return
				string.Compare(extension, ".baml", true) == 0	&&											// the resource has a .baml extension
				entry.Value is Stream							&&											// the resource is a Stream
				(string.IsNullOrEmpty(resourceName) || string.Compare(resourceName, entryName, true) == 0);	// the resource name requested equals to current resource name
		}

		#endregion
	}
}
