using System;
using System.Net;
namespace SharpVectors.Net
{
	public class DataWebRequestCreator : IWebRequestCreate
	{
		public DataWebRequestCreator()
		{
		}

		public WebRequest Create(Uri uri)
		{
			return new DataWebRequest(uri);
		}
	}
}
