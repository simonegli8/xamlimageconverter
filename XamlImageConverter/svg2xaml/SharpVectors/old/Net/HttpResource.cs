using System;
using System.IO;

namespace SharpVectors.Net
{
	public class HttpResource
	{
		public HttpResource(Uri uri, string contentType, Stream stream)
		{
			Uri = uri;
			ContentType = contentType;
			Stream = stream;
		}

		public Uri Uri;
		public string ContentType;
		public Stream Stream;
	}
}
