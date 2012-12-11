using System;
using SharpVectors.Dom.Svg;
using SharpVectors.Dom.Css;
using System.Windows.Media;

namespace SharpVectors.Renderer.Xaml
{
    public class GdiSvgPaint : SvgPaint
    {
        private SvgStyleableElement _element;
        private PaintServer ps;
        private XamlRenderer renderer;

        public GdiSvgPaint(XamlRenderer renderer, SvgStyleableElement elm, string propName)
            : base(elm.GetComputedStyle("").GetPropertyValue(propName))
        {
            _element = elm;
            this.renderer = renderer;
        }

        #region Private methods
        private int getOpacity(string fillOrStroke)
        {
            double alpha = 255;
            string opacity;

            opacity = _element.GetPropertyValue(fillOrStroke + "-opacity");
            if (opacity.Length > 0) alpha *= SvgNumber.ParseToFloat(opacity);

            opacity = _element.GetPropertyValue("opacity");
            if (opacity.Length > 0) alpha *= SvgNumber.ParseToFloat(opacity);

            alpha = Math.Min(alpha, 255);
            alpha = Math.Max(alpha, 0);

            return Convert.ToInt32(alpha);
        }

        private PenLineCap getLineCap()
        {
            switch (_element.GetPropertyValue("stroke-linecap"))
            {
                case "round":
                    return PenLineCap.Round;
                case "square":
                    return PenLineCap.Square;
                default:
                    return PenLineCap.Flat;
            }
        }

        private PenLineJoin getLineJoin()
        {
            switch (_element.GetPropertyValue("stroke-linejoin"))
            {
                case "round":
                    return PenLineJoin.Round;
                case "bevel":
                    return PenLineJoin.Bevel;
                default:
                    return PenLineJoin.Miter;
            }
        }

        private double getStrokeWidth()
        {
            string strokeWidth = _element.GetPropertyValue("stroke-width");
            if (strokeWidth.Length == 0) strokeWidth = "1px";

            SvgLength strokeWidthLength = new SvgLength("stroke-width", strokeWidth, _element, SvgLengthDirection.Viewport);
            return (double)strokeWidthLength.Value;
        }

        private double getMiterLimit()
        {
            string miterLimitStr = _element.GetPropertyValue("stroke-miterlimit");
            if (miterLimitStr.Length == 0) miterLimitStr = "4";

            double miterLimit = SvgNumber.ParseToFloat(miterLimitStr);
            //if (miterLimit < 1) throw new SvgException(SvgExceptionType.SvgInvalidValueErr, "stroke-miterlimit can not be less then 1");

            return miterLimit;
        }

        private double[] getDashArray(double strokeWidth)
        {
            string dashArray = _element.GetPropertyValue("stroke-dasharray");

            if (dashArray.Length == 0 || dashArray == "none")
            {
                return null;
            }
            else
            {
                SvgNumberList list = new SvgNumberList(dashArray);

                int len = list.NumberOfItems;
                double[] fDashArray = new double[len];

                for (int i = 0; i < len; i++)
                {
                    //divide by strokeWidth to take care of the difference between Svg and GDI+
                    fDashArray[i] = list.GetItem(i).Value / strokeWidth;
                }

                if (len % 2 == 1)
                {
                    //odd number of values, duplicate
                    double[] tmpArray = new double[len * 2];
                    fDashArray.CopyTo(tmpArray, 0);
                    fDashArray.CopyTo(tmpArray, (int)len);

                    fDashArray = tmpArray;
                }

                return fDashArray;
            }
        }

        private double getDashOffset(double strokeWidth)
        {
            string dashOffset = _element.GetPropertyValue("stroke-dashoffset");
            if (dashOffset.Length > 0)
            {
                //divide by strokeWidth to take care of the difference between Svg and GDI+
                SvgLength dashOffsetLength = new SvgLength("stroke-dashoffset", dashOffset, _element, SvgLengthDirection.Viewport);
                return (double)dashOffsetLength.Value;
            }
            else
            {
                return 0;
            }
        }
        private PaintServer getPaintServer(XamlRenderer renderer, string uri)
        {
            Uri absoluteUri = _element.ResolveUri(uri);
            return PaintServer.CreatePaintServer(renderer, _element.OwnerDocument, absoluteUri);
        }
        #endregion

        #region Public methods
        public Brush GetBrush(Geometry gp)
        {
            SvgPaint fill;
            if (PaintType == SvgPaintType.None)
            {
                return null;
            }
            else if (PaintType == SvgPaintType.CurrentColor)
            {
                fill = new GdiSvgPaint(renderer, _element, "color");
            }
            else
            {
                fill = this;
            }

            if (fill.PaintType == SvgPaintType.Uri ||
                fill.PaintType == SvgPaintType.UriCurrentColor ||
                fill.PaintType == SvgPaintType.UriNone ||
                fill.PaintType == SvgPaintType.UriRgbColor ||
                fill.PaintType == SvgPaintType.UriRgbColorIccColor)
            {
                Uri absoluteUri = _element.ResolveUri(fill.Uri);
                ps = getPaintServer(renderer, fill.Uri);
                if (ps != null)
                {
                    return ps.GetBrush(gp.Bounds);
                }
                else
                {
                    if (PaintType == SvgPaintType.UriNone ||
                        PaintType == SvgPaintType.Uri)
                    {
                        return null;
                    }
                    else if (PaintType == SvgPaintType.UriCurrentColor)
                    {
                        fill = new GdiSvgPaint(renderer, _element, "color");
                    }
                    else
                    {
                        fill = this;
                    }
                }
            }


            SolidColorBrush brush = new SolidColorBrush(((RgbColor)fill.RgbColor).GdiColor);
            int opacity = getOpacity("fill");
            brush.Color = Color.FromArgb((byte)opacity, brush.Color.R, brush.Color.G, brush.Color.B);
            return brush;
        }

        public Pen GetPen(Geometry gp)
        {
            double strokeWidth = getStrokeWidth();
            if (strokeWidth == 0) return null;

            SvgPaint stroke;
            if (PaintType == SvgPaintType.None)
            {
                return null;
            }
            else if (PaintType == SvgPaintType.CurrentColor)
            {
                stroke = new GdiSvgPaint(renderer, _element, "color");
            }
            else
            {
                stroke = this;
            }

            Pen pen = new Pen(Brushes.Black, strokeWidth);

            if (stroke.PaintType == SvgPaintType.Uri)
            {
                ps = getPaintServer(renderer, stroke.Uri);
                pen.Brush = ps.GetBrush(gp.Bounds);
            }
            else
            {
                Color color = ((RgbColor)stroke.RgbColor).GdiColor;
                int opacity = getOpacity("stroke");
                pen.Brush = new SolidColorBrush(Color.FromArgb((byte)opacity, color.R, color.G, color.B));
            }

            pen.StartLineCap = pen.EndLineCap = getLineCap();
            pen.DashCap = pen.StartLineCap;
            pen.LineJoin = getLineJoin();
            pen.MiterLimit = getMiterLimit();

            double[] fDashArray = getDashArray(strokeWidth);
            if (fDashArray != null)
            {
                pen.DashStyle = new DashStyle(fDashArray, getDashOffset(strokeWidth));
            }

            //pen.DashOffset = getDashOffset(strokeWidth);

            return pen;
        }

        #endregion

        #region Public properties
        public PaintServer PaintServer
        {
            get
            {
                return ps;
            }
        }

        #endregion
    }
}
