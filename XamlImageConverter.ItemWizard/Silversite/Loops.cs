using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;


namespace Silversite {

	public static class Loops {

		public static void For<T>(this IList<T> set, Action<IList<T>, int> action) { for (var i = 0; i < set.Count; i++) action(set, i); }

		public static void ForEach<T>(this IEnumerable<T> set, Action<T> action) { foreach (var x in set) action(x); }
		public static void Each<T>(this IEnumerable<T> set, Action<T> action) { foreach (var x in set) action(x); }
	}
}