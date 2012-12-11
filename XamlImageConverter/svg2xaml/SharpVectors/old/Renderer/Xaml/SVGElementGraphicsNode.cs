using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

using SharpVectors.Dom.Svg;
using SharpVectors.Dom.Svg.Rendering;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Collections;
using SharpVectorXamlRenderingEngine;

namespace SharpVectors.Renderer.Xaml
{
    /// <summary>
    /// Summary description for SvgElementGraphicsNode.
    /// </summary>
    public class SvgElementGraphicsNode : GraphicsNode
    {
        #region Constructor
        public SvgElementGraphicsNode(SvgElement element)
            : base(element)
        {
        }
        #endregion

        public override void BeforeRender(ISvgRenderer renderer)
        {
            if (this.Element.GetAttribute("display") == "none")
                return;

            base.BeforeRender(renderer);
        }


        #region Public Methods
        public override void Render(ISvgRenderer renderer)
        {
            if (this.Element.GetAttribute("display") == "none")
                return;

            Canvas canvas = ((XamlRenderer)renderer).Canvas;
            //GraphicsWrapper graphics = ((XamlRenderer) renderer).GraphicsWrapper;

            SvgSvgElement svgElm = (SvgSvgElement)element;
            //Rect viewBox = ((SvgRect)svgElm.ViewBox.AnimVal).GetGdiRectangle();

            double x = svgElm.X.AnimVal.Value;
            double y = svgElm.Y.AnimVal.Value;
            double width = svgElm.Width.AnimVal.Value;
            double height = svgElm.Height.AnimVal.Value;


            canvas.Width = width;
            canvas.Height = height;
            AssignValue(canvas, Canvas.LeftProperty, x, 0d);
            AssignValue(canvas, Canvas.TopProperty, y, 0d);
            
            //path.ClipToBounds = false;
            //path.Height = height;
            //path.Width = width;
            //Canvas.SetTop(path, x);
            //Canvas.SetLeft(path, y);

            Rect elmRect = new Rect(x, y, width, height);

            if (element.ParentNode is SvgElement)
            {
                // TODO: should it be moved with x and y?
            }

            // check overflow property
            string overflow = ((SvgSvgElement)element).GetPropertyValue("overflow");
            string clip = ((SvgSvgElement)element).GetPropertyValue("clip").Trim();

            if ((clip.Length == 0 || clip.Equals("auto")) && (overflow == "scroll" || overflow == "hidden" || overflow.Length == 0))
            {
                //graphics.SetClip(elmRect);
                canvas.ClipToBounds = true;
            }

            if (clip.Length > 0)
            {
                // only valid value is rect(top, right, bottom, left)
                if (clip.StartsWith("rect("))
                {
                    string rect = clip.Trim().Substring(5);
                    rect = rect.Substring(0, rect.Length - 1);
                    String[] dimensions = rect.Split(new char[] { ',' });

                    if (dimensions.Length == 4)
                    {
                        Rect clipRect = new Rect(
                            x + (double)System.Convert.ToInt32(dimensions[3]),
                            y + (double)System.Convert.ToInt32(dimensions[0]),
                            width - System.Convert.ToInt32(dimensions[1]),
                            height - System.Convert.ToInt32(dimensions[2]));

                        canvas.Clip = new RectangleGeometry(clipRect);
                        //graphics.SetClip(clipRect);                        
                    }
                    else
                    {
                        throw new SvgException(SvgExceptionType.SvgInvalidValueErr, "Invalid clip value");
                    }
                }

                var name = Utils.FormatWPFName(this.Element.Id);
                if (!String.IsNullOrEmpty(name))
                    canvas.Name = name;
                //path.Name = this.Element.Id;
                //canvas.Children.Add(path);
            }



            string sOpacity = svgElm.GetPropertyValue("opacity");
            double opacity = SvgNumber.ParseToFloat(sOpacity);
            AssignValue(canvas, UIElement.OpacityProperty, opacity, 1d);
            //fitToViewbox(graphics, elmRect);
        }
        #endregion
    }
}
