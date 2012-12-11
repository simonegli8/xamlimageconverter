using System;
using System.Xml;
using System.Net;
using System.IO;

using ICSharpCode.SharpZipLib.GZip;

namespace SharpVectors.Xml
{
	/// <summary>
	/// Summary description for GZipXmlTextReader.
	/// </summary>
	public class GZipXmlTextReader : XmlTextReader
	{
		private static Stream GetUnzippedStream(Stream stream)
		{
			MemoryStream memStream = new MemoryStream();
			byte[] buffer = new byte[4096];
			int nrOfReadByte = stream.Read(buffer, 0, 4096);
			while(nrOfReadByte > 0)
			{
				memStream.Write(buffer, 0, nrOfReadByte);
				nrOfReadByte = stream.Read(buffer, 0, 4096);
			}
			stream.Close();

			memStream.Position = 0;
			GZipInputStream gzipStream = new GZipInputStream(memStream);
			
			try
			{
				MemoryStream unzippedStream = new MemoryStream();
				
				buffer = new byte[4096];
				nrOfReadByte = gzipStream.Read(buffer, 0, 4096);
				while(nrOfReadByte > 0)
				{
					unzippedStream.Write(buffer, 0, nrOfReadByte);
					nrOfReadByte = gzipStream.Read(buffer, 0, 4096);
				}

				gzipStream.Close();
				memStream.Close();
				unzippedStream.Position = 0;
				// GZipped
				return unzippedStream;
			}
			catch
			{
				// not GZipped
				memStream.Position = 0;
				return memStream;
			}
		}

		private static Stream GetUnzippedStream(string url)
		{
			WebRequest req = WebRequest.Create(url);
			WebResponse resp = req.GetResponse();
			
			MemoryStream memStream = new MemoryStream();
			Stream responseStream = resp.GetResponseStream();

			byte[] buffer = new byte[4096];
			int nrOfReadByte = responseStream.Read(buffer, 0, 4096);
			while(nrOfReadByte > 0)
			{
				memStream.Write(buffer, 0, nrOfReadByte);
				nrOfReadByte = responseStream.Read(buffer, 0, 4096);
			}
            return GetUnzippedStream(memStream);
		}

		public GZipXmlTextReader(string url) : base(url, GZipXmlTextReader.GetUnzippedStream(url))
		{
		}

		public GZipXmlTextReader(Stream stream) : base(GZipXmlTextReader.GetUnzippedStream(stream))
		{
		}

	}
}
