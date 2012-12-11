using System;
using System.Collections;
using System.Diagnostics;
using System.Xml;
using SharpVectors.Dom.Svg;
using System.Windows.Media;
using System.Windows;
using System.Collections.Generic;

namespace SharpVectors.Renderer.Xaml
{
    /// <summary>
    /// Summary description for PaintServer.
    /// </summary>
    public class GradientPaintServer : PaintServer
    {
        public GradientPaintServer(XamlRenderer renderer, SvgGradientElement gradientElement)
            : base(renderer)
        {
            _gradientElement = gradientElement;
        }

        private SvgGradientElement _gradientElement;

        #region Private methods
        private List<Color> getColors(XmlNodeList stops)
        {
            List<Color> colors = new List<Color>(stops.Count);
            for (int i = 0; i < stops.Count; i++)
            {
                SvgStopElement stop = (SvgStopElement)stops.Item(i);
                string prop = stop.GetPropertyValue("stop-color");
                GdiSvgColor svgColor = new GdiSvgColor(stop, "stop-color");

                colors.Add(svgColor.Color);
            }

            return colors;
        }

        private List<double> getPositions(XmlNodeList stops)
        {
            List<double> positions = new List<double>(stops.Count);
            double lastPos = 0;
            for (int i = 0; i < stops.Count; i++)
            {
                SvgStopElement stop = (SvgStopElement)stops.Item(i);
                double pos = (double)stop.Offset.AnimVal;

                pos /= 100;
                pos = Math.Max(lastPos, pos);

                positions.Add(pos);
                lastPos = pos;
            }

            return positions;
        }

        private void correctPositions(List<double> positions, List<Color> colors)
        {
            if (positions.Count > 0)
            {
                double firstPos = (double)positions[0];
                if (firstPos > 0d)
                {
                    positions.Insert(0, 0d);
                    colors.Insert(0, colors[0]);
                }
                double lastPos = (double)positions[positions.Count - 1];
                if (lastPos < 1d)
                {
                    positions.Add(1d);
                    colors.Add(colors[colors.Count - 1]);
                }
            }
        }

        private void getColorsAndPositions(XmlNodeList stops, ref double[] positions, ref Color[] colors)
        {
            List<Color> alColors = getColors(stops);
            List<double> alPositions = getPositions(stops);

            if (alPositions.Count > 0)
            {
                correctPositions(alPositions, alColors);

                colors = alColors.ToArray();
                positions = alPositions.ToArray();
            }
            else
            {
                colors = new Color[2];
                colors[0] = Colors.Black;
                colors[1] = Colors.Black;

                positions = new double[2];
                positions[0] = 0;
                positions[1] = 1;
            }
        }

        private LinearGradientBrush GetLinearGradientBrush(SvgLinearGradientElement res, System.Windows.Rect bounds)
        {

            LinearGradientBrush brush = new LinearGradientBrush();
            Point startPt = new Point(res.X1.AnimVal.Value, res.Y1.AnimVal.Value);
            Point endPt = new Point(res.X2.AnimVal.Value, res.Y2.AnimVal.Value);

            brush.StartPoint = startPt;
            brush.EndPoint = endPt;

            if (res.GradientUnits.AnimVal.CompareTo(SvgUnitType.UserSpaceOnUse) == 0)
            {
                SharpVectors.Dom.Svg.Rendering.RenderingNode.AssignValue(brush, LinearGradientBrush.MappingModeProperty, BrushMappingMode.Absolute);
            }
            else if (res.GradientUnits.AnimVal.CompareTo(SvgUnitType.ObjectBoundingBox) == 0)
                SharpVectors.Dom.Svg.Rendering.RenderingNode.AssignValue(brush, LinearGradientBrush.MappingModeProperty, BrushMappingMode.RelativeToBoundingBox);            

            XmlNodeList stops = res.Stops;

            Color[] adjcolors = null;
            double[] adjpositions = null;
            getColorsAndPositions(stops, ref adjpositions, ref adjcolors);


            for (int i = 0; i < adjcolors.Length; i++)
            {
                brush.GradientStops.Add(new GradientStop(adjcolors[i], adjpositions[i]));
            }


            if (res.SpreadMethod.AnimVal.Equals(SvgSpreadMethod.Reflect))
            {
                brush.SpreadMethod = GradientSpreadMethod.Reflect;
            }
            else if (res.SpreadMethod.AnimVal.Equals(SvgSpreadMethod.Repeat))
            {
                brush.SpreadMethod = GradientSpreadMethod.Repeat;
            }
            else if (res.SpreadMethod.AnimVal.Equals(SvgSpreadMethod.Pad))
            {
                brush.SpreadMethod = GradientSpreadMethod.Pad;
            }

            brush.Transform = new MatrixTransform(getTransformMatrix(res));

            if (res.GetPropertyValue("color-interpolation") == "linearRGB")
            {
                brush.ColorInterpolationMode = ColorInterpolationMode.SRgbLinearInterpolation;
            }
            else
            {
                brush.ColorInterpolationMode = ColorInterpolationMode.ScRgbLinearInterpolation;
            }

            return brush;
        }

        private Matrix getTransformMatrix(SvgGradientElement gradientElement)
        {
            SvgMatrix svgMatrix = ((SvgTransformList)gradientElement.GradientTransform.AnimVal).TotalMatrix;

            Matrix transformMatrix = new Matrix(
                (double)svgMatrix.A,
                (double)svgMatrix.B,
                (double)svgMatrix.C,
                (double)svgMatrix.D,
                (double)svgMatrix.E,
                (double)svgMatrix.F);

            return transformMatrix;
        }

        private RadialGradientBrush GetRadialGradientBrush(SvgRadialGradientElement res, System.Windows.Rect bounds)
        {
            double fCenterX = (double)res.Cx.AnimVal.Value;
            double fCenterY = (double)res.Cy.AnimVal.Value;
            double fFocusX = (double)res.Fx.AnimVal.Value;
            double fFocusY = (double)res.Fy.AnimVal.Value;
            double fRadius = (double)res.R.AnimVal.Value;

            double fEffectiveCX = fCenterX;
            double fEffectiveCY = fCenterY;
            double fEffectiveFX = fFocusX;
            double fEffectiveFY = fFocusY;
            double fEffectiveRadiusX = fRadius;
            double fEffectiveRadiusY = fRadius;

            if (res.GradientUnits.AnimVal.Equals(SvgUnitType.ObjectBoundingBox))
            {
                fEffectiveCX = bounds.Left + fCenterX * (bounds.Width);
                fEffectiveCY = bounds.Top + fCenterY * (bounds.Height);
                fEffectiveFX = bounds.Left + fFocusX * (bounds.Width);
                fEffectiveFY = bounds.Top + fFocusY * (bounds.Height);
                fEffectiveRadiusX = fRadius * bounds.Width;
                fEffectiveRadiusY = fRadius * bounds.Height;
            }

            RadialGradientBrush brush = new RadialGradientBrush();
            brush.Center = new Point(fEffectiveFX, fEffectiveFY);

            if (res.GradientUnits.AnimVal.CompareTo(SvgUnitType.UserSpaceOnUse) == 0)
                SharpVectors.Dom.Svg.Rendering.RenderingNode.AssignValue(brush, LinearGradientBrush.MappingModeProperty, BrushMappingMode.Absolute);            
            else if (res.GradientUnits.AnimVal.CompareTo(SvgUnitType.ObjectBoundingBox) == 0)
                SharpVectors.Dom.Svg.Rendering.RenderingNode.AssignValue(brush, LinearGradientBrush.MappingModeProperty, BrushMappingMode.RelativeToBoundingBox);            
   
            brush.Center = new Point(fCenterX, fCenterY);
            brush.RadiusX = res.R.AnimVal.Value;
            brush.RadiusY = res.R.AnimVal.Value;
            brush.GradientOrigin = new Point(fCenterX, fCenterY);

            XmlNodeList stops = res.Stops;

            Color[] adjcolors = null;
            double[] adjpositions = null;
            getColorsAndPositions(stops, ref adjpositions, ref adjcolors);

            for (int i = 0; i < adjcolors.Length; i++)
            {
                brush.GradientStops.Add(new GradientStop(adjcolors[i], adjpositions[i]));
            }


            if (res.SpreadMethod.AnimVal.Equals(SvgSpreadMethod.Reflect))
            {
                brush.SpreadMethod = GradientSpreadMethod.Reflect;
            }
            else if (res.SpreadMethod.AnimVal.Equals(SvgSpreadMethod.Repeat))
            {
                brush.SpreadMethod = GradientSpreadMethod.Repeat;
            }
            else if (res.SpreadMethod.AnimVal.Equals(SvgSpreadMethod.Pad))
            {
                brush.SpreadMethod = GradientSpreadMethod.Pad;
            }


            brush.Transform = new MatrixTransform(getTransformMatrix(res));

            /*
             * How to do brush.GammaCorrection = true on a PathGradientBrush? / nikgus
             * */

            return brush;
        }
        #endregion

        #region Public methods
        public Geometry GetRadialGradientRegion(System.Windows.Rect bounds)
        {
            SvgRadialGradientElement res = _gradientElement as SvgRadialGradientElement;

            if (_gradientElement == null)
            {
                return null;
            }

            double fCenterX = (double)res.Cx.AnimVal.Value;
            double fCenterY = (double)res.Cy.AnimVal.Value;
            double fFocusX = (double)res.Fx.AnimVal.Value;
            double fFocusY = (double)res.Fy.AnimVal.Value;
            double fRadius = (double)res.R.AnimVal.Value;

            double fEffectiveCX = fCenterX;
            double fEffectiveCY = fCenterY;
            double fEffectiveFX = fFocusX;
            double fEffectiveFY = fFocusY;
            double fEffectiveRadiusX = fRadius;
            double fEffectiveRadiusY = fRadius;

            if (res.GradientUnits.AnimVal.Equals(SvgUnitType.ObjectBoundingBox))
            {
                fEffectiveCX = bounds.Left + fCenterX * (bounds.Width);
                fEffectiveCY = bounds.Top + fCenterY * (bounds.Height);
                fEffectiveFX = bounds.Left + fFocusX * (bounds.Width);
                fEffectiveFY = bounds.Top + fFocusY * (bounds.Height);
                fEffectiveRadiusX = fRadius * bounds.Width;
                fEffectiveRadiusY = fRadius * bounds.Height;
            }
            return new EllipseGeometry(
                new Point(fEffectiveCX, fEffectiveCY)
                , fEffectiveRadiusX, fEffectiveRadiusY);


            //GraphicsPath gp2 = new GraphicsPath();
            //gp2.AddEllipse(fEffectiveCX - fEffectiveRadiusX,fEffectiveCY - fEffectiveRadiusY,2 * fEffectiveRadiusX, 2 * fEffectiveRadiusY);

            //return new Region(gp2);
        }


        public override Brush GetBrush(System.Windows.Rect bounds)
        {
            string id = _gradientElement.Id;

            Brush brush;
            if (XamlRenderer.BrushCaches.TryGetValue(id, out brush))
                return brush;

            if (_gradientElement is SvgLinearGradientElement)
            {
                brush = GetLinearGradientBrush((SvgLinearGradientElement)_gradientElement, bounds);
            }
            else if (_gradientElement is SvgRadialGradientElement)
            {
                brush = GetRadialGradientBrush((SvgRadialGradientElement)_gradientElement, bounds);
            }
            else
            {
                brush = Brushes.Black;
            }
            XamlRenderer.AddBrush(id, brush);
            return brush;
        }

        #endregion
    }
}
