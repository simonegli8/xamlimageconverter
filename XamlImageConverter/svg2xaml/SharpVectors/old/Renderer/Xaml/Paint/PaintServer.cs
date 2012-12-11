using System;
using System.Collections;
using System.Diagnostics;
using System.Xml;
using SharpVectors.Dom.Svg;
using System.Windows.Media;
using System.Windows;

namespace SharpVectors.Renderer.Xaml
{
    public abstract class PaintServer
    {
        public static PaintServer CreatePaintServer(XamlRenderer renderer, SvgDocument document, Uri absoluteUri)
        {
            XmlNode node = document.GetNodeByUri(absoluteUri);

            if (node is SvgGradientElement)
            {
                return new GradientPaintServer(renderer, (SvgGradientElement)node);
            }
            else if (node is SvgPatternElement)
            {
                return new PatternPaintServer(renderer, (SvgPatternElement)node);
            }
            else
            {
                return null;
            }
        }
        private XamlRenderer xamlRenderer;

        public XamlRenderer XamlRenderer
        {
            get { return xamlRenderer; }
            set { xamlRenderer = value; }
        }


        protected PaintServer(XamlRenderer renderer)
        {
            this.xamlRenderer = renderer;
        }

        public abstract Brush GetBrush(Rect bounds);
    }
}
