using System;
using System.Windows.Media;

using System.Text.RegularExpressions;
using System.Xml;
using System.Windows;



namespace SharpVectors.Dom.Svg
{
    /// <summary>
    /// Summary description for SvgTextElement.
    /// </summary>
    public class SvgTextElement : SvgTextPositioningElement, ISvgTextElement
    {
        internal SvgTextElement(string prefix, string localname, string ns, SvgDocument doc)
            : base(prefix, localname, ns, doc)
        {
        }

        //private GraphicsPath gp = null;
        public override Geometry GetGeometry()
        {

            Point ctp = new Point(0, 0); // current text position

            return GetGeometry(ref ctp);

        }

    }
}
