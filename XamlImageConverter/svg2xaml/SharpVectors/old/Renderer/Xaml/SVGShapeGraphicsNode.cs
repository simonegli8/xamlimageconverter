using System;
using System.Collections.Generic;
using System.Text;
using SharpVectors.Dom.Svg.Rendering;
using SharpVectors.Dom.Svg;
using System.Windows.Shapes;
using SharpVectorXamlRenderingEngine;
using System.Windows;
using System.Windows.Media;

namespace SharpVectors.Renderer.Xaml
{
    public class SVGShapeGraphicsNode : GraphicsNode
    {
        public SVGShapeGraphicsNode(SvgElement element)
            : base(element)
        {
        }

        public override void Render(ISvgRenderer renderer)
        {
            if (this.Element.GetAttribute("display") == "none")
                return;

            XamlRenderer xamlRenderer = renderer as XamlRenderer;
            //GraphicsWrapper graphics = gdiRenderer.GraphicsWrapper;

            SvgStyleableElement styleElm = element as SvgStyleableElement;
            if (styleElm != null)
            {
                string sVisibility = styleElm.GetPropertyValue("visibility");
                string sDisplay = styleElm.GetPropertyValue("display");
                string sOpacity = styleElm.GetPropertyValue("opacity");
                double opacity = SvgNumber.ParseToFloat(sOpacity);

                if (element is ISharpShape && sVisibility != "hidden" && sDisplay != "none")
                {
                    Shape shape = ((ISharpShape)element).GetShape();

                    var name = Utils.FormatWPFName(this.Element.Id);
                    if (!String.IsNullOrEmpty(name))
                        shape.Name = name;

                    AssignValue(shape, UIElement.OpacityProperty, opacity, 1d);

                    Matrix transformMatrix = GraphicsNode.GetTransformMatrix(element);
                    if (!transformMatrix.IsIdentity)
                        shape.RenderTransform = new MatrixTransform(transformMatrix);

                    xamlRenderer.Canvas.Children.Add(shape);

                    //Clip(graphics);
                    Geometry geom = shape.RenderedGeometry;
                    GdiSvgPaint fillPaint = new GdiSvgPaint(xamlRenderer, styleElm, "fill");
                    Brush brush = fillPaint.GetBrush(geom);

                    GdiSvgPaint strokePaint = new GdiSvgPaint(xamlRenderer, styleElm, "stroke");
                    Pen pen = strokePaint.GetPen(geom);

                    if (brush != null)
                    {
                        shape.Fill = brush;
                    }

                    if (pen != null)
                    {                       
                        AssignValue(shape, Shape.StrokeProperty, pen.Brush);
                        AssignValue(shape, Shape.StrokeThicknessProperty, pen.Thickness);
                        AssignValue(shape, Shape.StrokeDashArrayProperty, pen.DashStyle.Dashes);
                        AssignValue(shape, Shape.StrokeDashCapProperty, pen.DashCap);
                        AssignValue(shape, Shape.StrokeDashOffsetProperty, pen.DashStyle.Offset);
                        AssignValue(shape, Shape.StrokeLineJoinProperty, pen.LineJoin);
                        AssignValue(shape, Shape.StrokeStartLineCapProperty, pen.StartLineCap);
                        AssignValue(shape, Shape.StrokeEndLineCapProperty, pen.EndLineCap);
                        AssignValue(shape, Shape.StrokeMiterLimitProperty, pen.MiterLimit);
                    }                    
                }

            }
        }
    }
}
