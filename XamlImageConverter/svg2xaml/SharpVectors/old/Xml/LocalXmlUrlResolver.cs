using System;
using System.Xml;
using System.IO;

namespace SharpVectors.Xml
{
	/// <summary>
	///		Filters out resolution of non-local Urls
	/// </summary>
	/// <developer>Rick.Bullotta@lighthammer.com</developer>
	/// <completed>100</completed>


	public class LocalXmlUrlResolver : XmlUrlResolver
	{
		public LocalXmlUrlResolver() 
		{
		}
 
		override public object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn) 
		{ 
			// If a remote URI, ignore it
			if(absoluteUri.Scheme.Equals("http") || absoluteUri.Scheme.Equals("http") || absoluteUri.Scheme.Equals("ftp")) 
			{
					return new MemoryStream();
			}
			string uri = absoluteUri.Host + absoluteUri.LocalPath;
 
			// Convert UNIX path delimiters to Win path delimiters
 
			uri = uri.Replace("/", "\\");

			// We ignore all DTD requests
  
			if ( uri.EndsWith(".dtd") ) 
			{
				return new MemoryStream();
			} 
			else 
			{
				// Not a DTD, so do the default behavior
				return base.GetEntity(absoluteUri, role, ofObjectToReturn);
			}
		}
	}
}

