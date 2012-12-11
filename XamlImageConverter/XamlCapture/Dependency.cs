using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;

namespace XamlImageConverter {

	public class Dependency: Group {

		public Dependency(XElement x) {
			FileInfo info = new FileInfo(Compiler.MapPath((string)x.Value));
			if (info.Exists) Version = info.LastWriteTimeUtc;
		}
	}
}
