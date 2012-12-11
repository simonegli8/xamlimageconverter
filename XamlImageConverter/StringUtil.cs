using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XamlImageConverter {
	public static class StringUtil {
		public static bool IsNullOrWhiteSpace(string text) {
			if (text == null) return true;
			for (int i = 0; i < text.Length; i++) { if (!char.IsWhiteSpace(text[i])) return false; }
			return true;
		}
	}
}
