using System;
using System.Diagnostics;
using System.Xml;

using SharpVectors.Dom.Svg;
using SharpVectors.Dom.Svg.Rendering;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SharpVectors.Renderer.Xaml
{
	public enum SvgMarkerPosition{Start, Mid, End}

	public class SvgMarkerGraphicsNode : GraphicsNode
	{
        #region Constructor
		public SvgMarkerGraphicsNode(SvgElement element) : base(element)
		{
		}
        #endregion

        #region Public Methods
        // disable default rendering
		public override void BeforeRender(ISvgRenderer renderer)
		{
		}
        public override void Render(ISvgRenderer renderer)
        {
        }
        public override void AfterRender(ISvgRenderer renderer)
        {
        }

        public void PaintMarker(XamlRenderer renderer, SvgMarkerPosition markerPos
            , SvgStyleableElement refElement, PathGeometry path)
        {
            //PathGeometry g;
            //g.GetPointAtFractionLength(

            ISharpMarkerHost markerHostElm = (ISharpMarkerHost)refElement;
            SvgMarkerElement markerElm = (SvgMarkerElement)element;

            Point[] vertexPositions = markerHostElm.MarkerPositions;
            int start;
            int len;

            // Choose which part of the position array to use
            switch (markerPos)
            {
                case SvgMarkerPosition.Start:
                    start = 0;
                    len = 1;
                    break;
                case SvgMarkerPosition.Mid:
                    start = 1;
                    len = vertexPositions.Length - 2;
                    break;
                default:
                    // == MarkerPosition.End
                    start = vertexPositions.Length - 1;
                    len = 1;
                    break;
            }

            for (int i = start; i < start + len; i++)
            {
                Point point = vertexPositions[i];

                Matrix m = GetTransformMatrix(element);
                    
                //GraphicsContainer gc = gr.BeginContainer();

                //gr.TranslateTransform(point.X, point.Y);

                if (markerElm.OrientType.AnimVal.Equals(SvgMarkerOrient.Angle))
                {
                    m.Rotate((double)markerElm.OrientAngle.AnimVal.Value);
                    //gr.RotateTransform((double)markerElm.OrientAngle.AnimVal.Value);
                }
                else
                {
                    double angle;

                    switch (markerPos)
                    {
                        case SvgMarkerPosition.Start:
                            angle = markerHostElm.GetStartAngle(i + 1);
                            break;
                        case SvgMarkerPosition.Mid:
                            //angle = (markerHostElm.GetEndAngle(i) + markerHostElm.GetStartAngle(i + 1)) / 2;
                            angle = SvgNumber.CalcAngleBisection(markerHostElm.GetEndAngle(i), markerHostElm.GetStartAngle(i + 1));
                            break;
                        default:
                            angle = markerHostElm.GetEndAngle(i);
                            break;
                    }
                    //gr.RotateTransform(angle);
                    m.Rotate(angle);
                }

                if (markerElm.MarkerUnits.AnimVal.Equals(SvgMarkerUnit.StrokeWidth))
                {
                    string propValue = refElement.GetPropertyValue("stroke-width");
                    if (propValue.Length == 0) propValue = "1";

                    SvgLength strokeWidthLength = new SvgLength("stroke-width", propValue, refElement, SvgLengthDirection.Viewport);
                    double strokeWidth = (double)strokeWidthLength.Value;
                    //gr.ScaleTransform(strokeWidth, strokeWidth);
                    m.Scale(strokeWidth, strokeWidth);
                }

                SvgPreserveAspectRatio spar = (SvgPreserveAspectRatio)markerElm.PreserveAspectRatio.AnimVal;
                double[] translateAndScale = spar.FitToViewBox(
                    (SvgRect)markerElm.ViewBox.AnimVal,
                    new SvgRect(
                        0,
                        0,
                        (double)markerElm.MarkerWidth.AnimVal.Value,
                        (double)markerElm.MarkerHeight.AnimVal.Value)
                    );


                //m.Translate(-(double)markerElm.RefX.AnimVal.Value * translateAndScale[2], -(double)markerElm.RefY.AnimVal.Value * translateAndScale[3]);
                //m.Scale(translateAndScale[2], translateAndScale[3]);
                m.Translate(point.X, point.Y);


                //Matrix oldTransform = TransformMatrix;
                //TransformMatrix = m;
                //try
                //{
                    //newTransform.Append(m);
                    //TransformGroup tg = new TransformGroup();

                    //renderer.Canvas.re

                    //gr.TranslateTransform(
                    //    -(double)markerElm.RefX.AnimVal.Value * translateAndScale[2],
                    //    -(double)markerElm.RefY.AnimVal.Value * translateAndScale[3]
                    //    );

                    //gr.ScaleTransform(translateAndScale[2], translateAndScale[3]);

                    markerElm.RenderChildren(renderer);
                //}
                //finally
                //{
                //    TransformMatrix = oldTransform;
                //}
            //    //gr.EndContainer(gc);
            }
        }
        #endregion
	}
}
