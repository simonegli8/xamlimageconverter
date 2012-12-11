using System;
using System.Text.RegularExpressions;
using System.Diagnostics;

using SharpVectors.Dom.Css;
using SharpVectors.Dom.Svg;
using SharpVectors.Dom.Svg.Rendering;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using SharpVectorXamlRenderingEngine;


namespace SharpVectors.Renderer.Xaml
{
	public class GDIPathGraphicsNode : GraphicsNode
	{
        #region Constructor
		public GDIPathGraphicsNode(SvgElement element) : base(element)
		{
		}
        #endregion
		
		#region Marker code
		private string extractMarkerUrl(string propValue)
		{
			Regex reUrl = new Regex(@"^url\((?<uri>.+)\)$");

			Match match = reUrl.Match(propValue);
			if(match.Success)
			{
				return match.Groups["uri"].Value;
			}
			else
			{
				return String.Empty;
			}
		}

		private void PaintMarkers(XamlRenderer renderer, SvgStyleableElement styleElm, PathGeometry path)
		{            
            if (styleElm is ISharpMarkerHost)
            {
                string markerStartUrl = extractMarkerUrl(styleElm.GetPropertyValue("marker-start", "marker"));
                string markerMiddleUrl = extractMarkerUrl(styleElm.GetPropertyValue("marker-mid", "marker"));
                string markerEndUrl = extractMarkerUrl(styleElm.GetPropertyValue("marker-end", "marker"));

                RenderingNode grNode;
                if (markerStartUrl.Length > 0)
                {
                    grNode = renderer.GetGraphicsNodeByUri(styleElm.BaseURI, markerStartUrl);
                    if (grNode is SvgMarkerGraphicsNode)
                    {
                        ((SvgMarkerGraphicsNode)grNode).PaintMarker(renderer, SvgMarkerPosition.Start, styleElm, path);
                    }
                }

                if (markerMiddleUrl.Length > 0)
                {
                    // TODO markerMiddleUrl != markerStartUrl
                    grNode = renderer.GetGraphicsNodeByUri(styleElm.BaseURI, markerMiddleUrl);
                    if (grNode is SvgMarkerGraphicsNode)
                    {
                        ((SvgMarkerGraphicsNode)grNode).PaintMarker(renderer, SvgMarkerPosition.Mid, styleElm, path);
                    }
                }

                if (markerEndUrl.Length > 0)
                {
                    // TODO: markerEndUrl != markerMiddleUrl
                    grNode = renderer.GetGraphicsNodeByUri(styleElm.BaseURI, markerEndUrl);

                    if (grNode is SvgMarkerGraphicsNode)
                    {
                        ((SvgMarkerGraphicsNode)grNode).PaintMarker(renderer, SvgMarkerPosition.End, styleElm, path);
                    }
                }
            }
		}
		#endregion

		#region Private methods
        //private Brush GetBrush(PathGeometry gp)
        //{
        //    GdiSvgPaint paint = new GdiSvgPaint(element as SvgStyleableElement, "fill");
        //    return paint.GetBrush(gp);
        //}

        //private Pen GetPen(PathGeometry gp)
        //{
        //    GdiSvgPaint paint = new GdiSvgPaint(element as SvgStyleableElement, "stroke");
        //    return paint.GetPen(gp);
        //}
		#endregion

		#region Public methods

        public override void BeforeRender(ISvgRenderer renderer)
        {
            base.BeforeRender(renderer);
        }

        //public override void BeforeRender(ISvgRenderer renderer)
        //{
        //    //GraphicsWrapper graphics = ((XamlRenderer) renderer).GraphicsWrapper;

        //    //graphicsContainer = graphics.BeginContainer();
        //    //SetQuality(graphics);
        //    Transform();
        //}

        public override void Render(ISvgRenderer renderer)
        {
            XamlRenderer xamlRenderer = renderer as XamlRenderer;
            //GraphicsWrapper graphics = gdiRenderer.GraphicsWrapper;

            if (!(element is SvgClipPathElement) && !(element.ParentNode is SvgClipPathElement))
            {
                SvgStyleableElement styleElm = element as SvgStyleableElement;
                if (styleElm != null)
                {
                    string sVisibility = styleElm.GetPropertyValue("visibility");
                    string sDisplay = styleElm.GetPropertyValue("display");
                    string sOpacity = styleElm.GetPropertyValue("opacity");
                    double opacity = SvgNumber.ParseToFloat(sOpacity);

                    if (element is ISharpPathGeometry && sVisibility != "hidden" && sDisplay != "none")
                    {
                        Path path = new Path();
                        var name = Utils.FormatWPFName(this.Element.Id);
                        if (!String.IsNullOrEmpty(name))
                            path.Name = name;
                        AssignValue(path, UIElement.OpacityProperty, opacity, 1d);
                        
                        Matrix transformMatrix = GetTransformMatrix(element);
                        if(!transformMatrix.IsIdentity)
                            path.RenderTransform = new MatrixTransform(transformMatrix);

                        xamlRenderer.Canvas.Children.Add(path);
                        Geometry gp = ((ISharpPathGeometry)element).GetGeometry();                        
                        path.Data = gp;

                        if (gp != null)
                        {
                            //Clip(graphics);

                            GdiSvgPaint fillPaint = new GdiSvgPaint(xamlRenderer, styleElm, "fill");
                            Brush brush = fillPaint.GetBrush(gp);

                            GdiSvgPaint strokePaint = new GdiSvgPaint(xamlRenderer, styleElm, "stroke");
                            Pen pen = strokePaint.GetPen(gp);

                            if (brush != null)
                            {
                                path.Fill = brush;
                                //if (brush is GradientBrush)
                                //{
                                //    GradientPaintServer gps = fillPaint.PaintServer as GradientPaintServer;
                                    
                                //    path.Clip = gps.GetRadialGradientRegion(gp.Bounds);
                                //    path.Fill = brush;
                                    //GraphicsContainer container = graphics.BeginContainer();
                                    
                                    //graphics.SetClip(gps.GetRadialGradientRegion(gp.Bounds), CombineMode.Exclude);
                                    

                                    //SolidBrush tempBrush = new SolidBrush(((PathGradientBrush)brush).InterpolationColors.Colors[0]);
                                    //graphics.FillPath(this, tempBrush, gp);
                                    //tempBrush.Dispose();
                                    //graphics.ResetClip();

                                    //graphics.EndContainer(container);
                                //}

                                //graphics.FillPath(this, brush, gp);
                                //brush.Dispose();
                            }

                            if (pen != null)
                            {
                                if (pen.Brush is GradientBrush)
                                {
                                    //GradientPaintServer gps = strokePaint.PaintServer as GradientPaintServer;
                                    //GraphicsContainer container = graphics.BeginContainer();

                                    //graphics.SetClip(gps.GetRadialGradientRegion(gp.GetBounds()), CombineMode.Exclude);

                                    //SolidBrush tempBrush = new SolidBrush(((PathGradientBrush)pen.Brush).InterpolationColors.Colors[0]);
                                    //Pen tempPen = new Pen(tempBrush, pen.Width);
                                    //graphics.DrawPath(this, tempPen, gp);
                                    //tempPen.Dispose();
                                    //tempBrush.Dispose();

                                    //graphics.EndContainer(container);
                                }

                                //graphics.DrawPath(this, pen, gp);
                                //pen.Dispose();

                                AssignValue(path, Shape.StrokeProperty, pen.Brush);
                                AssignValue(path, Shape.StrokeThicknessProperty, pen.Thickness);
                                AssignValue(path, Shape.StrokeDashArrayProperty, pen.DashStyle.Dashes);
                                AssignValue(path, Shape.StrokeDashCapProperty, pen.DashCap);
                                AssignValue(path, Shape.StrokeDashOffsetProperty, pen.DashStyle.Offset);
                                AssignValue(path, Shape.StrokeLineJoinProperty, pen.LineJoin);
                                AssignValue(path, Shape.StrokeStartLineCapProperty, pen.StartLineCap);
                                AssignValue(path, Shape.StrokeEndLineCapProperty, pen.EndLineCap);
                                AssignValue(path, Shape.StrokeMiterLimitProperty, pen.MiterLimit);
                                //path.Stroke = pen.Brush;
                                //path.StrokeThickness = pen.Thickness;
                                //path.StrokeDashArray = pen.DashStyle.Dashes;
                                //path.StrokeDashCap = pen.DashCap;
                                //path.StrokeDashOffset = pen.DashStyle.Offset;
                                //path.StrokeLineJoin = pen.LineJoin;
                                //path.StrokeStartLineCap = pen.StartLineCap;
                                //path.StrokeEndLineCap = pen.EndLineCap;
                                //path.StrokeMiterLimit = pen.MiterLimit;
                            }
                            PaintMarkers(xamlRenderer, styleElm, gp.GetFlattenedPathGeometry());                            
                        }
                    }
                }
            }
		}
		#endregion

	}
}
