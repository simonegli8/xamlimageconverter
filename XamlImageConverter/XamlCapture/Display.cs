using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;

namespace XamlImageConverter {
	
	public class Display {

		public static void Show(FrameworkElement e, Rect bounds = default(Rect)) {
#if DEBUG
/*
			var w = new Window();
			var c = new Canvas();
			c.Children.Add(e);
			if (bounds != default(Rect)) {
				var r = new Rectangle();
				r.Width = bounds.Width;
				r.Height = bounds.Height;
				Canvas.SetTop(r, bounds.Top);
				Canvas.SetLeft(r, bounds.Left);
				r.Stroke = new SolidColorBrush(Colors.Red);
				c.Children.Add(r);
			}
			w.Content = c;
			w.SizeToContent = SizeToContent.WidthAndHeight;
			w.Show();
*/
#endif
		}
	}
}
