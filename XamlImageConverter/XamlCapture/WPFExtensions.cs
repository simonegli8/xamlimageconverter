using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Reflection;

namespace XamlImageConverter {

	public static class WPFExtensions {
		/// <summary>
		/// Measure and Arrange an element using the specified maximum size
		/// </summary>
		/// <param name="element">Element to Measure and Arrange</param>
		/// <param name="size">Maximum container size</param>
		/// <returns>The Measured and Arranged element</returns>
		public static UIElement MeasureAndArrange(this UIElement element, Size size) {
			element.Measure(size);
			element.Arrange(new Rect(element.DesiredSize));
			element.UpdateLayout();

			if (element is Page && ((Page)element).Content is Canvas) {
				var page = (Page)element;
				var canvas = (Canvas)page.Content;
				if (page.ActualWidth == 0 || page.ActualHeight == 0) { // photoshop import has a canvas without size
					size = new Size(0, 0);
					foreach (UIElement part in canvas.Children) {
						if (part is FrameworkElement) {
							var fp = (FrameworkElement)part;
							double top, left;
							top = (double)fp.GetValue(Canvas.TopProperty);
 							left = (double)fp.GetValue(Canvas.LeftProperty);
							size = new Size(Math.Max(size.Width, left + fp.Width), Math.Max(size.Height, top + fp.Height));
						}
					}

					page.Width = size.Width; page.Height = size.Height;
					element.Measure(size);
					element.Arrange(new Rect(element.DesiredSize));
					element.UpdateLayout();
				}
			}

			return element;
		}

		/// <summary>
		/// Find an element by its name; throw an exception if it doesn't exist
		/// </summary>
		/// <typeparam name="T">Element type</typeparam>
		/// <param name="rootElement">Root element</param>
		/// <param name="name">Element name</param>
		/// <returns>Returns the element</returns>
		public static T FindName<T>(this DependencyObject element, string name, int level = 0) where T: DependencyObject {
			//var element = (T)rootElement.FindName(name);
			//if (element != null) return element;

			//LogicalTreeHelper.FindLogicalNode(element, name);

			T res = null;
			var type = element.GetType();
			var svgid = SharpVectors.Runtime.SvgObject.GetId(element);

			/*
			var names = type.GetCustomAttributes(true)
				.OfType<System.Windows.Markup.RuntimeNamePropertyAttribute>()
				.Select(a => type.GetProperty(a.Name))
				.Where(p => p != null)
				.Select(p => p.GetValue(element, new object[0]) as string)
				.ToList();
			if (svgid != null) names.Add(svgid);

			if (names.Count > 0) for (int i = 0; i < level; i++) Console.Write("  ");
			foreach (var n in names) Console.Write("\"{0}\" ,", n);
			if (names.Count > 0) Console.WriteLine();
			*/

			if (type.GetCustomAttributes(true)
				.OfType<System.Windows.Markup.RuntimeNamePropertyAttribute>()
				.Select(a => type.GetProperty(a.Name))
				.Any(p => p != null && p.GetValue(element, new object[0]) as string == name)
				) return (T)element;

			if (svgid == name) return (T)element;

			if (element is Image) {
				res = FindName<T>(((Image)element).Source, name, level + 1);
			} else if (element is DrawingImage) {
				res = FindName<T>(((DrawingImage)element).Drawing, name, level + 1);
			} else if (element is DrawingGroup) {
				res = ((DrawingGroup)element).Children
					.Select(child => child.FindName<T>(name, level + 1))
					.FirstOrDefault(child => child != null);
			}
			if (res != null) return res;

			res = LogicalTreeHelper.GetChildren(element)
				.OfType<DependencyObject>()
				.Select(child => child.FindName<T>(name, level+1))
				.FirstOrDefault(child => child != null);
			if (res != null) return res;

			if (level == 0) throw new CompilerException(string.Format("The specified element '{0}' does not exist", name), 19, null, null, null);
			return null;
		}

		public static IEnumerable<string> FindAllNames(this DependencyObject element) {
			//var element = (T)rootElement.FindName(name);
			//if (element != null) return element;

			//LogicalTreeHelper.FindLogicalNode(element, name);

			var type = element.GetType();
			var svgid = SharpVectors.Runtime.SvgObject.GetId(element);
			if (svgid != null) yield return svgid;

			var names = type.GetCustomAttributes(true)
				.OfType<System.Windows.Markup.RuntimeNamePropertyAttribute>()
				.Select(a => type.GetProperty(a.Name))
				.Where(p => p != null)
				.Select(p => p.GetValue(element, new object[0]) as string)
				.Where(n => !string.IsNullOrEmpty(n));

			if (element is Image) {
				names = names.Union(((Image)element).Source.FindAllNames());
			} else if (element is DrawingImage) {
				names = names.Union(((DrawingImage)element).Drawing.FindAllNames());
			} else if (element is DrawingGroup) {
				names = names.Union(((DrawingGroup)element).Children
					.SelectMany(child => child.FindAllNames()));
			}

			names = names.Union(LogicalTreeHelper.GetChildren(element)
				.OfType<DependencyObject>()
				.SelectMany(child => child.FindAllNames()));

			foreach (var name in names) yield return name;
		}


		/// <summary>
		/// Find a resource by its name; throw an exception if it doesn't exist
		/// </summary>
		/// <typeparam name="T">Resource type</typeparam>
		/// <param name="rootElement">Root element</param>
		/// <param name="name">Resource name</param>
		/// <returns>Returns the resource</returns>
		public static T FindResource<T>(this FrameworkElement rootElement, string name) {
			return (T)rootElement.FindResource(name);
		}

		public static void Disconnect(this FrameworkElement e) {
			if (e.Parent != null) {
				var m = typeof(FrameworkElement).GetMethod("RemoveLogicalChild", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
				m.Invoke(e.Parent, new object[1] { e });
			
			}
		}

		public static Rect Bounds(this FrameworkElement e, FrameworkElement from) {
			var bounds = new Rect(0, 0, e.ActualWidth, e.ActualHeight);
			if (from == null) return bounds;
			var t = e.TransformToVisual(from);
			bounds = t.TransformBounds(bounds);
			t = e.LayoutTransform;
			bounds = t.TransformBounds(bounds);
			return bounds;
		}

		/*
		/// <summary>
		/// Returns the ancestors of a DependencyObject
		/// </summary>
		/// <param name="element">Child element</param>
		/// <param name="includeVisual">True to include Visual elements, otherwise false</param>
		/// <returns>Returns the ancestors of the specified <see cref="DependencyObject"/></returns>
		public static IEnumerable<DependencyObject> Ancestors(this DependencyObject element, bool includeVisual) {
			if (element == null) yield break;

			var group = element as Group;
			
			var parent = (group != null && includeVisual)
					? (group.Element ?? group.Parent as DependencyObject)
					: group.Parent as DependencyObject;

				if (parent != null) foreach (var p in Ancestors(parent, includeVisual)) yield return p;
				yield return element;
			}
		} */

		public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> list, int n) {
			int i = 0, j = 0;
			var buf = new T[n];
			foreach (T item in list) {
				buf[j] = item;
				j = (j+1)%n;
				if (i==j) {
					yield return buf[i];
					i = (i+1)%n;
				}
			}
		}
		public static IEnumerable<T> Append<T>(this IEnumerable<T> list, params T[] items) { return list.Concat(items); }
		public static IEnumerable<T> Prepend<T>(this IEnumerable<T> list, params T[] items) { return items.Concat(list); }

	}
}
