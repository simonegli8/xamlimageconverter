using System;
using System.Xml;
using System.Net;
using System.IO;
using SharpVectors.Net;

namespace SharpVectors.Xml
{
    public class CachedXmlDocument : CustomXmlDocument
    {
        private HttpCache cache;

        #region Constructors
        public CachedXmlDocument()
            : base()
        {
            cache = new HttpCache();
            XmlResolver = new CachingXmlUrlResolver();
        }

        public CachedXmlDocument(XmlNameTable nt)
            : base(nt)
        {
            cache = new HttpCache();
            XmlResolver = new CachingXmlUrlResolver();
        }

        #endregion

        #region Handling of referenced resources
        public override HttpResource GetResource(Uri absoluteUri)
        {
            HttpResource resource = cache.Get(absoluteUri);
            if (resource != null)
            {
                return resource;
            }
            else
            {
                WebRequest req = null;
                WebResponse resp = null;
                Stream respStream = null;

                if (absoluteUri.ToString().StartsWith("data:;base64,"))
                {
                    respStream = new MemoryStream(Convert.FromBase64String(absoluteUri.ToString().Substring("data:;base64,".Length)));
                }
                else if (absoluteUri.ToString().StartsWith("data:image/png;base64,"))
                {
                    respStream = new MemoryStream(Convert.FromBase64String(absoluteUri.ToString().Substring("data:image/png;base64,".Length)));
                }
                else
                {

                    req = WebRequest.Create(absoluteUri);
                    try
                    {
                        resp = req.GetResponse();
                        respStream = resp.GetResponseStream();
                    }
                    catch (System.Net.WebException)
                    {
                        return null;
                    }
                }

                bool cacheResult = cache.Add(absoluteUri, respStream, resp);
                if (cacheResult)
                {
                    // file got cached
                    return cache.Get(absoluteUri);
                }
                else
                {
                    return new HttpResource(absoluteUri, resp != null ? resp.ContentType : String.Empty, respStream);
                }
            }

        }


        #endregion
    }
}
