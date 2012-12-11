using System;
using System.IO;
using System.Xml;
using System.Net;

using SharpVectors.Net;

namespace SharpVectors.Xml
{
	public class CustomXmlDocument : XmlDocument
	{
		#region Constructors
		public CustomXmlDocument() : base()
		{
		}

		public CustomXmlDocument(XmlNameTable nt) : base(nt)
		{
		}
		#endregion

		public virtual HttpResource GetResource(Uri absoluteUri)
		{
			WebRequest req = WebRequest.Create(absoluteUri);
			WebResponse resp = req.GetResponse();

			return new HttpResource(absoluteUri, resp.ContentType, resp.GetResponseStream());
		}
	}
}
