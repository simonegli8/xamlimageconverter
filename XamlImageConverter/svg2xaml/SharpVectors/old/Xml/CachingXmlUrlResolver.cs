using System;
using System.Xml;
using System.IO;
using System.Net;

using SharpVectors.Net;

namespace SharpVectors.Xml
{


    public class CachingXmlUrlResolver : XmlUrlResolver
    {
        private HttpCache cache;

        public CachingXmlUrlResolver()
        {
            cache = new HttpCache();
        }

        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
        {
            HttpResource resource = cache.Get(absoluteUri);
            if (resource != null)
            {
                return resource.Stream;
            }
            else
            {

                WebRequest req = WebRequest.Create(absoluteUri);
                WebResponse resp = req.GetResponse();
                Stream respStream = resp.GetResponseStream();

                bool cacheResult = cache.Add(absoluteUri, respStream, resp);
                if (cacheResult)
                {
                    // file got cached
                    return cache.Get(absoluteUri).Stream;
                }
                else
                {
                    return respStream;
                }

            }
        }

        public override Uri ResolveUri(Uri baseUri, string relativeUri)
        {
            return base.ResolveUri(baseUri, relativeUri);
        }
    }
}