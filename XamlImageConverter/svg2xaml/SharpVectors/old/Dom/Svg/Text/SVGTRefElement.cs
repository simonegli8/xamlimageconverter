using System;
using System.Xml;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows;




namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// Summary description for SvgTRefElement.
	/// </summary>
	public class SvgTRefElement : SvgTextPositioningElement, ISvgTRefElement
	{
		internal SvgTRefElement(string prefix, string localname, string ns, SvgDocument doc) : base(prefix, localname, ns, doc) 
		{
			svgURIReference = new SvgURIReference(this);
		}

		public string GetText()
		{
			XmlElement refElement = ReferencedElement;
			if(refElement != null)
			{
				return TrimText(refElement.InnerText);
			}
			else
			{
				return String.Empty;
			}
		}

		protected override GeometryGroup GetGeometry(ref Point ctp)
		{
            GeometryGroup gp = new GeometryGroup();

			ctp = this.GetCurrentTextPosition(this, ctp);
			
			this.AddGraphicsPath(ref ctp, GetText(), gp);
            return gp;
		}

		#region Implementation of ISvgURIReference
		private SvgURIReference svgURIReference;
		public ISvgAnimatedString Href
		{
			get
			{
				return svgURIReference.Href;
			}
		}

		public XmlElement ReferencedElement
		{
			get
			{
				return svgURIReference.ReferencedNode as XmlElement;
			}
		}
		#endregion
	}
}
