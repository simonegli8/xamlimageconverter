using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Xml;

using SharpVectors.Collections;
using SharpVectors.Dom.Events;
using SharpVectors.Dom.Svg;
using SharpVectors.Dom.Svg.Rendering;
using SharpVectors.Xml;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Collections.Generic;
using System.IO;

namespace SharpVectors.Renderer.Xaml
{
    /// <summary>
    /// Renders a Svg image to Xaml
    /// </summary>
    public class XamlRenderer : ISvgRenderer
    {
        #region Private fields
        private int counter = 0;
        private Hashtable graphicsNodes = new Hashtable();

        private TypeDictionary nodeByTagName;
        private Color backColor;
        private SvgWindow window;

        internal Color _getNextColor(GraphicsNode grNode)
        {
            int id = ++counter;
            int r = id >> 16;
            id -= r << 16;
            int g = id >> 8;
            id -= g << 8;
            int b = id;
            Color color = Color.FromRgb((byte)r, (byte)g, (byte)b);

            graphicsNodes.Add(color, grNode);
            return color;
        }

        private GraphicsNode _getGraphicsNodeFromColor(Color color)
        {
            if (color.A == 0) return null;
            else return (GraphicsNode)graphicsNodes[color];
        }


        //private IEventTarget currentTarget = null;
        private static int ColorToId(Color color)
        {
            int r = color.R;
            int g = color.G;
            int b = color.B;
            r = r << 16;
            g = g << 8;

            return r + g + b;
        }

        private Canvas canvas = new Canvas();

        public Canvas Canvas
        {
            get { return canvas; }            
            set { canvas = value; }         
        }



        #endregion

        #region Properties
        //public Bitmap RasterImage
        //{
        //    get { return rasterImage; }
        //}

        public SvgWindow Window
        {
            get { return window; }
            set { window = value; }
        }

        public Color BackColor
        {
            get { return backColor; }
            set { backColor = value; }
        }


        #endregion

        #region Constructor
        public XamlRenderer()
        {
            nodeByTagName = new TypeDictionary();
            string ns = SvgDocument.SvgNamespace;

            SetTagNameNodeType(ns, "svg", typeof(SvgElementGraphicsNode));
            SetTagNameNodeType(ns, "image", typeof(SvgImageElementGraphicsNode));
            SetTagNameNodeType(ns, "use", typeof(SvgUseElementGraphicsNode));
            SetTagNameNodeType(ns, "marker", typeof(SvgMarkerGraphicsNode));
            SetTagNameNodeType(ns, "g", typeof(SVGGroupElementGraphicsNode));
            
            backColor = Colors.White;
        }
        #endregion

        #region Public methods
        public ISvgRenderer GetRenderer()
        {
            return new XamlRenderer();
        }

        public void SetTagNameNodeType(string prefix, string localName, Type type)
        {
            nodeByTagName[prefix + ":" + localName] = type;
        }

        public RenderingNode GetRenderingNode(XmlElement node)
        {
            string name = node.NamespaceURI + ":" + node.LocalName;
            RenderingNode result;

            if (nodeByTagName.ContainsKey(name))
            {
                object[] args = new object[] { (SvgElement)node };
                result = (RenderingNode)nodeByTagName.CreateInstance(name, args);
            }
            else if (node is ISharpShape)
            {
                result = new SVGShapeGraphicsNode((SvgElement)node);
            }
            else if (node is ISharpPathGeometry)
            {
                result = new GDIPathGraphicsNode((SvgElement)node);
            }
            else
            {
                result = new GraphicsNode((SvgElement)node);
            }

            return result;
        }

        public RenderingNode GetGraphicsNodeByUri(string baseUri, string url)
        {
             
            //Uri absoluteUri = new Uri(new Uri(baseUri), url);
            Uri absoluteUri = ((SvgDocument)Window.Document).ResolveUri(baseUri, url);
            XmlElement elm = ((SvgDocument)Window.Document).GetNodeByUri(absoluteUri) as XmlElement;

            if (elm != null)
            {
                return GetRenderingNode(elm);
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// BeforeRender - Make sure we have a Graphics object to render to.  If we don't have one, then create one to matche the SvgWindow's physical dimensions.
        /// </summary>
        internal void BeforeRender()
        {
            // Testing for null here allows "advanced" developers to create their own Graphics object for rendering
            //if (graphics == null)
            //{
            //    // Get the current SVGWindow's width and height
            //    int innerWidth = (int)window.InnerWidth;
            //    int innerHeight = (int)window.InnerHeight;

            //    // Make sure we have an actual area to render to
            //    if (innerWidth > 0 && innerHeight > 0)
            //    {
            //        // See if we already have a rasterImage that matches the current SVGWindow dimensions
            //        if (rasterImage == null || rasterImage.Width != innerWidth || rasterImage.Height != innerHeight)
            //        {
            //            // Nope, so create one
            //            rasterImage = new Bitmap(innerWidth, innerHeight);
            //        }

            //        // Make a GraphicsWrapper object from the rasterImage and clear it to the background color
            //        graphics = GraphicsWrapper.FromImage(rasterImage);
            //        graphics.Clear(backColor);
            //    }
            //}
        }

        /// <summary>
        /// AfterRender - Dispose of Graphics object created for rendering.
        /// </summary>
        private void AfterRender()
        {
            //idMapRaster = graphics.IdMapRaster;
            //graphics.Dispose();
            //graphics = null;
        }

        public Canvas Render(SvgElement node)
        {
            BeforeRender();
            node.Render(this);
            AfterRender();
            return Canvas;
        }

        public Canvas Render(SvgDocument node)
        {
            BeforeRender();
            node.Render(this);
            AfterRender();
            return Canvas;
        }


        //public void Save(Stream stream)
        //{
        //    SharpVectorXamlRenderingEngine.XamlWriter.Save(Canvas, stream, resources);
        //    //System.Windows.Markup.XamlWriter.Save(Canvas, stream);
        //}
        #endregion

        //public void OnMouseEvent(string type, MouseEventArgs e)
        //{
        //    if (idMapRaster != null)
        //    {
        //        try
        //        {
        //            Color pixel = idMapRaster.GetPixel(e.X, e.Y);
        //            GraphicsNode grNode = _getGraphicsNodeFromColor(pixel);
        //            if (grNode != null)
        //            {
        //                IEventTarget target = grNode.Element as IEventTarget;
        //                if (target != null)
        //                {
        //                    Event ev;
        //                    switch (type)
        //                    {
        //                        case "mousemove":
        //                            if (currentTarget == target)
        //                            {
        //                                ev = new Event("mousemove", true, false);
        //                                target.DispatchEvent(ev);
        //                            }
        //                            else
        //                            {
        //                                if (currentTarget != null)
        //                                {
        //                                    ev = new Event("mouseout", true, false);
        //                                    currentTarget.DispatchEvent(ev);
        //                                }
        //                                ev = new Event("mouseover", true, false);
        //                                target.DispatchEvent(ev);
        //                            }
        //                            break;
        //                        case "mousedown":
        //                            ev = new Event("mousedown", true, false);
        //                            target.DispatchEvent(ev);
        //                            break;
        //                        case "mouseup":
        //                            ev = new Event("mouseup", true, false);
        //                            target.DispatchEvent(ev);
        //                            ev = new Event("click", true, false);
        //                            target.DispatchEvent(ev);
        //                            break;
        //                    }

        //                    currentTarget = target;
        //                }
        //                else
        //                {
        //                    currentTarget = null;
        //                }
        //            }
        //            else
        //            {
        //                currentTarget = null;
        //            }
        //        }
        //        catch { }
        //    }
        //}

        private Dictionary<string, Brush> brushCaches = new Dictionary<string,Brush>();
        private Dictionary<object, string> resources = new Dictionary<object, string>();

        public Dictionary<string, Brush> BrushCaches
        {
            get { return brushCaches; }            
        }

        public bool UseBrushCache { get; set; }


        public void AddBrush(string key, Brush brush)
        {
            if (UseBrushCache)
            {
                brushCaches.Add(key, brush);
                AddResource(key, brush);
            }
        }

        public void AddResource(string key, object val)
        {
            Canvas.Resources.Add(key, val);
            resources[val] = key;
        }
    }
}
