using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Threading;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Markup;

using SharpVectors.Renderers.Wpf;
using SharpVectors.Converters;
using SharpVectors.Renderers.Utils;
using SharpVectors.Dom.Svg;

namespace SvgConvert {

	public static class ConvertUtility {
		
		public static FrameworkElement LoadSvg(string filename) {
			var settings = new WpfDrawingSettings();
			settings.IncludeRuntime = true;
			var renderer = new WpfDrawingRenderer(settings);
			renderer.LinkVisitor = new LinkVisitor();
			renderer.ImageVisitor = new EmbeddedImageVisitor();
			renderer.FontFamilyVisitor = new FontFamilyVisitor();
			var svgWindow = new WpfSvgWindow(4096, 4096, renderer);
			svgWindow.LoadDocument(filename);
			svgWindow.Resize((int)svgWindow.Document.RootElement.Width.BaseVal.Value, (int)svgWindow.Document.RootElement.Height.BaseVal.Value);
			renderer.InvalidRect = SvgRectF.Empty;
			renderer.Render(svgWindow.Document as SvgDocument);
			var drawing = new System.Windows.Media.DrawingImage();
			drawing.Drawing = renderer.Drawing;
			var image = new System.Windows.Controls.Image();
			image.Source = drawing;
			image.Width = renderer.Window.InnerWidth;
			image.Height = renderer.Window.InnerHeight;
			return image;
		}

		public static void ConvertSvg(string filename, string destfile) {
			var settings = new WpfDrawingSettings();
			settings.IncludeRuntime = false;
			var converter = new FileSvgConverter(settings);
			converter.Convert(filename, destfile);
		}
		
	}
}
