using System;
using System.Xml;

using SharpVectors.Dom.Css;


namespace SharpVectors.Dom.Svg
{
	public class SvgFitToViewBox
	{
		public SvgFitToViewBox(SvgElement ownerElement)
		{
			this.ownerElement = ownerElement;

			this.ownerElement.AttributeChange += new AttributeChangeHandler(onAttributeChange);
		}

		#region Private fields
		private SvgElement ownerElement;
		#endregion

		#region Update handling
		private void onAttributeChange(XmlNodeChangedAction action, XmlAttribute attribute)
		{
			if(attribute.NamespaceURI.Length == 0)
			{
				switch(attribute.LocalName)
				{
					case "viewBox":
						viewBox = null;
						break;
					case "preserveAspectRatio":
						preserveAspectRatio = null;
						break;
				}
			}
		}
		#endregion

		private ISvgAnimatedRect viewBox;
		public ISvgAnimatedRect ViewBox
		{
			get
			{
				if(viewBox == null)
				{
					string attr = ownerElement.GetAttribute("viewBox").Trim();
					if(attr.Length == 0)
					{
						double x = 0;
						double y = 0;
						double width = 0;
						double height = 0;
						if(ownerElement is SvgSvgElement)
						{
							SvgSvgElement svgSvgElm = ownerElement as SvgSvgElement;

							x = svgSvgElm.X.AnimVal.Value;
							y = svgSvgElm.Y.AnimVal.Value; 
							width = svgSvgElm.Width.AnimVal.Value;
							height = svgSvgElm.Height.AnimVal.Value;
							
						}
						viewBox = new SvgAnimatedRect(new SvgRect(x, y, width, height));
					}
					else
					{
						viewBox = new SvgAnimatedRect(attr);
					}
				}
				return viewBox;
			}
		}

		private ISvgAnimatedPreserveAspectRatio preserveAspectRatio;
		public ISvgAnimatedPreserveAspectRatio PreserveAspectRatio
		{
			get
			{
				if(preserveAspectRatio == null)
				{
					preserveAspectRatio = new SvgAnimatedPreserveAspectRatio(ownerElement.GetAttribute("preserveAspectRatio"));
				}
				return preserveAspectRatio;
			}
		}

	}
}
