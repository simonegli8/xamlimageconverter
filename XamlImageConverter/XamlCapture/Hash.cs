using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Text;
using System.IO;

namespace XamlImageConverter {

	public class Cache {

		public static string Path { get; set; }

		static XNamespace ns = Parser.ns1;

		public static string File(string id, XElement xaml, string type = null, string path = null) {
			var e = xaml.DescendantsAndSelf(ns + id).FirstOrDefault();
			if (e != null) {
				if (e.Attribute(ns + "ImageType") != null) type = (string)e.Attribute(ns + "ImageType");
				if (e.Attribute(ns + "Cache") != null) path = (string)e.Attribute(ns + "Cache").Value;
			}
			path = path ?? Path;
			if (type == null) type = "png";
			if (!path.EndsWith("/")) path += "/";
			return path + id + "." + Hash.Compute(xaml).ToString() + "." + type;
		}
	}

#if !Silversite
	public class Hash {

		public static int Compute(byte[] data) {
			unchecked {
				const int p = 16777619;
				int hash = (int)2166136261;

				for (int i = 0; i < data.Length; i++)
					hash = (hash ^ data[i]) * p;

				hash += hash << 13;
				hash ^= hash >> 7;
				hash += hash << 3;
				hash ^= hash >> 17;
				hash += hash << 5;
				hash &= 0x7FFFFFFF;
				return hash;
			}
		}

		public static int Compute(string s) { return Compute(System.Text.Encoding.UTF8.GetBytes(s)); }

		public static int Compute(XElement e) {
			using (var m = new MemoryStream())
			using (var w = XmlWriter.Create(m, new XmlWriterSettings() { NewLineChars = "", Encoding = Encoding.UTF8, Indent = false })) {
				e.Save(w);
				return Compute(m.GetBuffer());
			}
		}
	}

#else
			public class Hash: Silversite.Services.Hash {
			}
#endif
}
