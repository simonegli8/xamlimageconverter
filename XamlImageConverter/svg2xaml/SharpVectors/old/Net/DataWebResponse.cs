using System;
using System.Net;
using System.IO;

namespace SharpVectors.Net
{
	/// <summary>
	/// Summary description for DataWebResponse.
	/// </summary>
	/// <remarks>According to http://www.ietf.org/rfc/rfc2397.txt</remarks>
	public class DataWebResponse : WebResponse
	{
		private string data;
		private string mediaType;
		private bool isBase64 = false;
		private byte[] decodedData;

		internal DataWebResponse(Uri uri)
		{
			responseUri = uri;

			if(uri.Scheme.Equals("data"))
			{
				string all = uri.AbsoluteUri;
				int dataStart = all.IndexOf(",");
				if(dataStart < 5)
				{
					throw new Exception("Malformed data URI");
				}
				else
				{
					mediaType = all.Substring(5, dataStart - 5);
					data = all.Substring(dataStart + 1);
					
					if(mediaType.EndsWith(";base64"))
					{
						isBase64 = true;
						mediaType = mediaType.Remove(mediaType.Length - 7, 7);
						decodedData = Convert.FromBase64String(data);
					}

					if(mediaType.Length == 0)
					{
						mediaType = "text/plain;charset=US-ASCII";
					}
				}
			}
			else
			{
				throw new Exception("Unknown scheme");
			}
		}

		public override long ContentLength
		{
			get
			{
				if(isBase64)
				{
					return decodedData.Length;
				}
				else
				{
					return data.Length;
				}
			}
		}

		public override string ContentType
		{
			get
			{
				return mediaType;
			}
		}

        private Uri responseUri;
		public override Uri ResponseUri
		{
			get
			{
				return responseUri;
			}
		}

		public override Stream GetResponseStream()
		{
			MemoryStream ms; 
			if(isBase64)
			{
				ms = new MemoryStream(decodedData, false);
			}
			else
			{
				char[] chars = data.ToCharArray();
				ms = new MemoryStream(chars.Length);
				for(int i = 0; i<chars.Length; i++)
				{
					ms.WriteByte((byte)chars[i]);
				}
			}
			ms.Position = 0;
			return ms;
		}
	}
}
