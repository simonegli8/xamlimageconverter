using System;
using System.Net;

namespace SharpVectors.Net
{
	public class DataWebRequest : System.Net.WebRequest
	{
		public DataWebRequest(Uri uri)
		{
			requestUri = uri;
		}

		private Uri requestUri;
		public override Uri RequestUri
		{
			get
			{
				return requestUri;
			}
		}

		public override WebResponse GetResponse()
		{
			return new DataWebResponse(RequestUri);
		}
	}
}
