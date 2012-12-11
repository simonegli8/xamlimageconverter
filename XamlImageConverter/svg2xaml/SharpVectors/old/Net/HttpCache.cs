using System;
using System.Net;
using System.Xml;
using System.IO;
//using System.Windows.Forms;

namespace SharpVectors.Net
{
    public class HttpCache
    {
        static HttpCache()
        {
            IsCacheEnabled = true;
        }

        #region Private properties
        private string _cachePath;
        private string _cacheDataFile;
        private XmlDocument cacheData = new XmlDocument();

        public static bool IsCacheEnabled { get; set; }
        private bool isCacheEnabled;
        #endregion

        #region Constructors
        public HttpCache()
            : this(null)
        {
        }

        public HttpCache(string cachePath)
        {
            isCacheEnabled = IsCacheEnabled;
            try
            {
                if (cachePath == null)
                    cachePath = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)).FullName + @"\XamlImageConverter\cache";

                _cachePath = cachePath;
                _cacheDataFile = _cachePath + @"\cache.xml";


                if (!Directory.Exists(_cachePath))
                {
                    Directory.CreateDirectory(cachePath);
                }
                if (File.Exists(_cacheDataFile))
                {
                    cacheData.Load(_cacheDataFile);
                    cleanCache();
                }
                else
                {
                    cacheData = getEmptyDataDoc();
                }
            }
            catch
            {
                isCacheEnabled = false;
            }
        }
        #endregion

        #region Private methods
        private void cleanCache()
        {
            XmlNodeList fileNodes = cacheData.SelectNodes("/cachedFiles/file");
            foreach (XmlElement fileElm in fileNodes)
            {
                DateTime dt;
                if (!fileElm.HasAttribute("expires") ||
                    (DateTime.TryParse(fileElm.GetAttribute("expires"), out dt)
                    && dt < DateTime.Now))
                {
                    removeFile(fileElm);
                }
            }
        }

        private XmlDocument getEmptyDataDoc()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<cachedFiles />");
            return doc;
        }

        private void removeFile(XmlElement fileElm)
        {
            string filePath = _cachePath + @"\" + fileElm.GetAttribute("localpath");
            if (File.Exists(filePath)) File.Delete(filePath);
            fileElm.ParentNode.RemoveChild(fileElm);
        }

        private void addToXmlAndSave(Uri uri, string fileName, string mimeType, DateTime expires)
        {
            XmlElement fileElm = cacheData.CreateElement("file");
            fileElm.SetAttribute("uri", uri.AbsoluteUri);
            fileElm.SetAttribute("localpath", fileName);
            fileElm.SetAttribute("mimetype", mimeType);
            fileElm.SetAttribute("expires", expires.ToString());

            cacheData.DocumentElement.AppendChild(fileElm);
            cacheData.Save(_cacheDataFile);
        }
        #endregion

        #region Public methods
        public DateTime CacheUntil(Uri absoluteUri, WebHeaderCollection headers)
        {
            if (!isCacheEnabled)
                return DateTime.MinValue;

            if (absoluteUri.Scheme == "file")
            {
                // do not cache local files
                return DateTime.MinValue;
            }
            else if (absoluteUri.Query.Length > 0)
            {
                // do not cache URLs with querystrings
                return DateTime.MinValue;
            }
            // default: cache for one day
            DateTime until = DateTime.Now.AddDays(1);

            string[] expires = headers.GetValues("Expires");
            if (expires != null && expires.Length == 1)
            {
                until = DateTime.Parse(expires[0]);
            }

            string[] cacheControls = headers.GetValues("Cache-Control");
            if (cacheControls != null)
            {
                foreach (string cc in cacheControls)
                {
                    if (cc.Equals("no-cache") ||
                        cc.Equals("no-store") ||
                        cc.Equals("must-revalidate"))
                    {
                        return DateTime.MinValue;
                    }
                    else if (cc.StartsWith("max-age="))
                    {
                        int seconds = Int32.Parse(cc.Remove(0, 8));
                        until = DateTime.Now.AddSeconds(seconds);
                    }
                }
            }

            return until;
        }

        public bool Add(Uri absoluteUri, Stream inStream, WebResponse response)
        {
            return Add(absoluteUri, inStream, response != null ? response.ContentType : string.Empty, response != null ? CacheUntil(absoluteUri, response.Headers) : DateTime.Now.AddDays(1));
        }

        public bool Add(Uri absoluteUri, Stream inStream, String mimeType, DateTime expires)
        {
            if (!isCacheEnabled)
                return false;

            if (expires > DateTime.Now)
            {
                string fileName = Guid.NewGuid().ToString() + ".cached"; ;
                string filePath = _cachePath + @"\" + fileName;

                FileStream fs = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                byte[] buffer = new byte[4096];

                int readBytes = inStream.Read(buffer, 0, 4096);
                while (readBytes > 0)
                {
                    fs.Write(buffer, 0, readBytes);
                    readBytes = inStream.Read(buffer, 0, 4096);
                }

                fs.Flush();
                fs.Close();

                addToXmlAndSave(absoluteUri, fileName, mimeType, expires);
                return true;
            }
            else
            {
                return false;
            }
        }

        public double Size
        {
            get
            {
                if (!isCacheEnabled)
                    return 0;

                DirectoryInfo di = new DirectoryInfo(_cachePath);
                FileInfo[] files = di.GetFiles();
                double size = 0;
                foreach (FileInfo file in files)
                {
                    size += file.Length;
                }
                return size;
            }
        }

        public void Clear()
        {
            if (!isCacheEnabled)
                return;

            Directory.Delete(_cachePath, true);
            Directory.CreateDirectory(_cachePath);

            cacheData = getEmptyDataDoc();
        }


        public bool IsCached(Uri absoluteUri)
        {
            if (!isCacheEnabled)
                return false;

            DateTime dt;
            XmlElement fileElm = (XmlElement)cacheData.SelectSingleNode("/cachedFiles/file[@uri='" + absoluteUri.AbsoluteUri + "']");
            if (fileElm == null)
            {
                return false;
            }
            else if (!fileElm.HasAttribute("expires") ||
                (DateTime.TryParse(fileElm.GetAttribute("expires"), out dt) && dt < DateTime.Now))
            {
                removeFile(fileElm);
                return false;
            }
            else
            {
                return true;
            }
        }

        public HttpResource Get(Uri absoluteUri)
        {
            if (!IsCached(absoluteUri))
            {
                return null;
            }
            else
            {
                XmlElement fileElm = cacheData.SelectSingleNode("/cachedFiles/file[@uri='" + absoluteUri.AbsoluteUri + "']") as XmlElement;

                string filePath = _cachePath + @"\" + fileElm.GetAttribute("localpath");
                if (File.Exists(filePath))
                {
                    return new HttpResource(absoluteUri, fileElm.GetAttribute("mimetype"), new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read));
                }
                else
                {
                    removeFile(fileElm);
                    return null;
                }
            }
        }
        #endregion
    }
}
