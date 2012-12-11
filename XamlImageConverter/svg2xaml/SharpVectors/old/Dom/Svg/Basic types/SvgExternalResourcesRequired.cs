using System;
using System.Xml;

using SharpVectors.Dom.Css;

namespace SharpVectors.Dom.Svg
{
	public class SvgExternalResourcesRequired
	{
		public SvgExternalResourcesRequired(SvgElement ownerElement)
		{
			this.ownerElement = ownerElement;

			this.ownerElement.AttributeChange += new AttributeChangeHandler(onAttributeChange);
		}
		private SvgElement ownerElement;


		private void onAttributeChange(XmlNodeChangedAction action, XmlAttribute attribute)
		{
			if(attribute.LocalName == "externalResourcesRequired")
			{
				externalResourcesRequired = null;
			}
		}

		private ISvgAnimatedBoolean externalResourcesRequired;
		public ISvgAnimatedBoolean ExternalResourcesRequired
		{
			get
			{
				if(externalResourcesRequired == null)
				{
					externalResourcesRequired = new SvgAnimatedBoolean(ownerElement.GetAttribute("externalResourcesRequired"), false);
				}
				return externalResourcesRequired;
			}
		}
	
	}
}
