using System;
using System.Xml;
using System.Collections;
using System.Windows.Media;

namespace SharpVectors.Dom.Svg
{
	public enum SvgSpreadMethod{
		Pad, 
		Reflect, 
		Repeat
	}

	public class SvgGradientElement : SvgStyleableElement, ISvgGradientElement
	{
		internal SvgGradientElement(string prefix, string localname, string ns, SvgDocument doc)
			: base(prefix, localname, ns, doc)
		{
			svgURIReference = new SvgURIReference(this);
			svgExternalResourcesRequired = new SvgExternalResourcesRequired(this);
		}

		#region Implementation of ISvgGradientElement
		private ISvgAnimatedEnumeration gradientUnits;
		public ISvgAnimatedEnumeration GradientUnits
		{
			get
			{
				if(!HasAttribute("gradientUnits") && ReferencedElement != null)
				{
					return ReferencedElement.GradientUnits;
				}
				else
				{
					if(gradientUnits == null)
					{
						SvgUnitType gradUnit;
						switch(GetAttribute("gradientUnits"))
						{
							case "userSpaceOnUse":
								gradUnit = SvgUnitType.UserSpaceOnUse;
								break;
							default:
								gradUnit = SvgUnitType.ObjectBoundingBox;
								break;
						}

						gradientUnits = new SvgAnimatedEnumeration(gradUnit);
					}
					return gradientUnits;
				}
			}
		}

		private ISvgAnimatedTransformList gradientTransform;
		public ISvgAnimatedTransformList GradientTransform
		{
			get
			{
				if(!HasAttribute("gradientTransform") && ReferencedElement != null)
				{
					return ReferencedElement.GradientTransform;
				}
				else
				{
					if(gradientTransform == null)
					{
						gradientTransform = new SvgAnimatedTransformList(GetAttribute("gradientTransform"));
					}
					return gradientTransform;
				}
			}
		}

		private ISvgAnimatedEnumeration spreadMethod;
		public ISvgAnimatedEnumeration SpreadMethod
		{
			get
			{
				if(!HasAttribute("spreadMethod") && ReferencedElement != null)
				{
					return ReferencedElement.SpreadMethod;
				}
				else
				{
					if(spreadMethod == null)
					{
						SvgSpreadMethod spreadMeth;
						switch(GetAttribute("spreadMethod"))
						{
							case "reflect":
								spreadMeth = SvgSpreadMethod.Reflect;
								break;
							case "repeat":
								spreadMeth = SvgSpreadMethod.Repeat;
								break;
							default:
								spreadMeth = SvgSpreadMethod.Pad;
								break;
						}

						spreadMethod = new SvgAnimatedEnumeration(spreadMeth);
					}
					return spreadMethod;
				}
			}
		}

		#endregion

		#region Implementation of ISvgURIReference
		private SvgURIReference svgURIReference;
		public ISvgAnimatedString Href
		{
			get
			{
				return svgURIReference.Href;
			}
		}

		public SvgGradientElement ReferencedElement
		{
			get
			{
				return svgURIReference.ReferencedNode as SvgGradientElement;
			}
		}
		#endregion

		#region Implementation of ISvgExternalResourcesRequired
		private SvgExternalResourcesRequired svgExternalResourcesRequired;
		public ISvgAnimatedBoolean ExternalResourcesRequired
		{
			get
			{
				return svgExternalResourcesRequired.ExternalResourcesRequired;
			}
		}
		#endregion

		#region Other public fields
		public XmlNodeList Stops
		{
			get
			{
				XmlNodeList stops = SelectNodes("svg:stop", OwnerDocument.NamespaceManager);
				if(stops.Count > 0)
				{
					return stops;
				}
				else
				{
					// check any eventually referenced gradient
					if(ReferencedElement == null)
					{
						// return an empty list
						return stops;
					}
					else
					{
						return ReferencedElement.Stops;
					}
				}
			}
		}
		#endregion

		#region Update handling
		public override void OnAttributeChange(XmlNodeChangedAction action, XmlAttribute attribute)
		{
			base.OnAttributeChange(action, attribute);

			if(attribute.NamespaceURI.Length == 0)
			{
				switch(attribute.LocalName)
				{
					case "gradientUnits":
						gradientUnits = null;
						break;
					case "gradientTransform":
						gradientTransform = null;
						break;
					case "spreadMethod":
						spreadMethod = null;
						break;
				}
			}
		}
		#endregion
	}
}
