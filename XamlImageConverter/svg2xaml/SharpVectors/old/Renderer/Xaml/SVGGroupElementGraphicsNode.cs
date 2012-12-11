using System;
using System.Xml;

using SharpVectors.Dom.Svg;
using SharpVectors.Dom.Svg.Rendering;
using System.Windows.Media;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Text;
using System.Windows;
using SharpVectorXamlRenderingEngine;

namespace SharpVectors.Renderer.Xaml
{
    /// <summary>
    /// Summary description for SvgImageGraphicsNode.
    /// </summary>
    public class SVGGroupElementGraphicsNode : GraphicsNode
    {
        #region Constructor
        public SVGGroupElementGraphicsNode(SvgElement element)
            : base(element)
        {
        }
        #endregion

        public override void Render(ISvgRenderer renderer)
        {            
            if (this.Element.GetAttribute("display") == "none")
                return;

            base.Render(renderer);
        }

        private static Regex regExName = new Regex(@"^(\p{Lu}|\p{Ll}|\p{Lo}|\p{Lt}|\p{Nl}|_)((\p{Lu}|\p{Ll}|\p{Lo}|\p{Lt}|\p{Nl}|_)|\p{Nd}|\p{Mc}|\p{Mn} )*", RegexOptions.Compiled);


        public override void BeforeRender(ISvgRenderer renderer)
        {
            if (this.Element.GetAttribute("display") == "none")
                return;
            Canvas canvas = ((XamlRenderer)renderer).Canvas;
            Canvas cv = new Canvas();

            var name = Utils.FormatWPFName(this.Element.Id);
            if (!String.IsNullOrEmpty(name))
                cv.Name = name;

            string inkScapeLabel = this.Element.GetAttribute("label", "http://www.inkscape.org/namespaces/inkscape");
            if (!string.IsNullOrEmpty(inkScapeLabel))
            {
                try
                {
                    inkScapeLabel = Utils.FormatWPFName(inkScapeLabel);
                    var inkScapeName = Utils.FormatWPFName(inkScapeLabel);
                    if (!String.IsNullOrEmpty(inkScapeName))
                        canvas.Name = inkScapeName;
                }
                catch
                {
                }
            }

            canvas.Children.Add(cv);
            ((XamlRenderer)renderer).Canvas = cv;

            Matrix transformMatrix = GetTransformMatrix(element);
            if (!transformMatrix.IsIdentity)
                cv.RenderTransform = new MatrixTransform(transformMatrix);

            SvgGElement svgElm = (SvgGElement)element;
            string sOpacity = svgElm.GetPropertyValue("opacity");
            double opacity = SvgNumber.ParseToFloat(sOpacity);
            AssignValue(cv, UIElement.OpacityProperty, opacity, 1d);

            base.BeforeRender(renderer);
        }

        public override void AfterRender(ISvgRenderer renderer)
        {
            if (this.Element.GetAttribute("display") == "none")
                return;

            base.AfterRender(renderer);

            ((XamlRenderer)renderer).Canvas = (Canvas)((XamlRenderer)renderer).Canvas.Parent;
        }
    }
}

