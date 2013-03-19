using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace Silversite.Services {

	public static class Files {

		public static FileInfo FileInfo(string path) { return new FileInfo(Paths.Map(path)); }
		public static DirectoryInfo DirectoryInfo(string path) { return new DirectoryInfo(Paths.Map(path)); }

		static IEnumerable<string> AllRecursive(DirectoryInfo dir, string patterns) {
			return
				dir.Exists ?
					dir.GetDirectories().SelectMany(d => AllRecursive(d, patterns))
						.Union(AllLocal(dir, patterns))
					: new string[0];
		}
		static IEnumerable<string> AllLocal(DirectoryInfo dir, string patterns) {
			return
				dir.Exists ? 
					dir.GetFiles()
						.Select(f => Paths.Unmap(f.FullName))
						.Where(path => Paths.Match(patterns, path))
					: new string[0];
		}
		public static IEnumerable<string> All(string patterns) {
			return patterns
				.Tokens(s => Paths.Directory(s))
				.Select(s => s.Contains('*') ? Paths.Directory(s.UpTo('*')) : s)
				.Select(s => string.IsNullOrEmpty(s) ? "~/" : s)
				.Distinct()
				//.Where(d => !d.Contains("*"))
				.Select(d => DirectoryInfo(d))
				.SelectMany(dir => AllRecursive(dir, patterns))
				.Distinct();
		}
		public static IEnumerable<string> DirectoryAll(string patterns) {
			return patterns
				.Tokens(s => Paths.Directory(s))
				.Select(s => s.Contains('*') ? Paths.Directory(s.UpTo('*')) : s)
				.Select(s => string.IsNullOrEmpty(s) ? "~/" : s)
				.Distinct()
				.Select(d => DirectoryInfo(d))
				.SelectMany(dir => AllLocal(dir, patterns))
				.Distinct();
		}

		public static void Copy(string src, string dest) {
			if (src.Contains(";") || src.Contains('*') || src.Contains('?')) All(src).Each(f => Copy(f, Paths.Combine(dest, Paths.File(f))));
			else {
				if (src == dest) return;
				if (Directory.Exists(src)) {
					var info = DirectoryInfo(src);
					dest = Paths.Combine(dest, info.Name);
					Directory.CreateDirectory(dest);
					foreach (var obj in info.EnumerateFileSystemInfos()) Copy(Paths.Combine(src, obj.Name), dest);
				} else {
					if (Directory.Exists(dest)) File.Copy(Paths.Map(src), Paths.Map(Paths.Combine(dest, Paths.File(src))), true);
					else File.Copy(Paths.Map(src), Paths.Map(dest), true);
				}
			}
		}

	}
}