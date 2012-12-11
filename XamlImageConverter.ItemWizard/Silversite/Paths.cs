using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Silversite.Services {

	public class Paths {

		public static void Split(string path, out string directory, out string name) {
			int i = path.LastIndexOf('\\');
			if (i >= 0 && i < path.Length-1) {
				directory = path.Substring(0, i);
				name = path.Substring(i+1);
			} else {
				directory = string.Empty;
				name = path;
			}
		}

		public static string Directory(string path) {
			if (string.IsNullOrEmpty(path) || path == "~") return "~";
			string dir, file;
			Split(path, out dir, out file);
			return dir;
		}

		public static string File(string path) {
			if (string.IsNullOrEmpty(path) || path == "~") return "";
			string dir, file;
			Split(path, out dir, out file);
			return file;
		}

		public static string FileWithoutExtension(string path) {
			int i = path.LastIndexOf('.');
			int j = path.LastIndexOf('\\');
			if (j < 0) j = 0;
			if (i <= j) i = path.Length;
			return path.Substring(j, i - j);
		}

		public static string Combine(string path1, string path2) {
			if (path2.StartsWith("~")) path2 = path2.Substring(1);
			if (path1.EndsWith("\\")) {
				if (path2.StartsWith("\\")) path2 = path2.Substring(1);
				return path1 + path2;
			} else if (path2.StartsWith("\\")) return path1 + path2;
			return path1 + "\\" + path2;
		}

		public static string Map(string path) { return path; }
		public static string Unmap(string path) { return path; }

		public static string Extension(string path) {
			var name = File(path);
			int i = name.LastIndexOf('.');
			if (i > 0) return name.Substring(i+1);
			return string.Empty;
		}

		public static string WithoutExtension(string path) {
			int i = path.LastIndexOf('.');
			int j = path.LastIndexOf('\\');
			if (i > 0 && i > j) return path.Substring(0, i);
			return path;
		}

		public static string ChangeExtension(string path, string ext) {
			if (ext.StartsWith(".")) return WithoutExtension(path) + ext;
			else return WithoutExtension(path) + "." + ext;
		}

		private static bool MatchSingle(string pattern, string path) {
			var star = pattern.IndexOf('*');
			var slash = path.LastIndexOf('\\');
			if (!pattern.Contains('\\') && slash != -1) path = path.Substring(slash+1);
			if (star != -1) {
				return (star == 0 || path.StartsWith(pattern.Substring(0, star))) && (star == pattern.Length-1 || path.EndsWith(pattern.Substring(star+1)));
			} else {
				return pattern == path;
			}
		}
		/// <summary>
		/// Checks wether the path matches one of a comma or semicolon separated list of file patterns or a single file pattern.
		/// </summary>
		/// <param name="patterns">A comma or semicolon separared list of patterns or a single pattern</param>
		/// <param name="path">The path to check.</param>
		/// <returns>True if one of the patterns matches the path.</returns>
		public static bool Match(string patterns, string path) {
			foreach (var p in patterns.SplitList(';', ',')) {
				if (MatchSingle(p, path)) return true;
			}
			return false;
		}

		public static bool ExcludeMatch(string patterns, string path) {
			var file = Paths.File(path);
			foreach (var p in patterns.SplitList(';', ',')) {
				if (p == file || MatchSingle(p, path)) return true;
			}
			return false;
		}

	}
}