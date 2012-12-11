using System;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Collections;

namespace SharpVectors.Xml
{
	/// <summary>
	/// Used to redirect external DTDs to local copies.
	/// </summary>
	public class LocalDtdXmlUrlResolver : XmlUrlResolver
	{
		public LocalDtdXmlUrlResolver()
		{
		}

		private Hashtable knownDtds = new Hashtable();

		public void AddDtd(string publicIdentifier, string localFile)
		{
            knownDtds.Add(publicIdentifier, localFile);
		}

		override public object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn) 
		{ 
			if(absoluteUri != null && knownDtds.ContainsKey(absoluteUri.AbsoluteUri))
			{
				//string cb = System.IO.Directory.GetParent(System.Windows.Forms.Application.ExecutablePath).FullName;
				var cb = System.Reflection.Assembly.GetExecutingAssembly().CodeBase
					.Substring("file:///".Length)
					.Replace('/', '\\');
				if (cb.StartsWith(System.Environment.GetFolderPath(Environment.SpecialFolder.Windows))) cb = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"\MSBuild\JohnsHope Software\XamlImageConverter\2.4");
				cb = cb.Substring(0, cb.LastIndexOf("\\"));

				string localFile = Path.Combine(cb, (string)knownDtds[absoluteUri.AbsoluteUri]);
				try
				{
					return new FileStream(localFile, FileMode.Open, FileAccess.Read, FileShare.Read );
				}
				catch
				{
					throw new Exception("Unable to load the specified local DTD: " + localFile);
				}
			}
			else
			{			
				return base.GetEntity(absoluteUri, role, ofObjectToReturn);
			}
		}
	}
}
