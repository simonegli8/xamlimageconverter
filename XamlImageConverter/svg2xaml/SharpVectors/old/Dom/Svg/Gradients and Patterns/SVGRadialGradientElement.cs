using System;
using System.Xml;
using SharpVectors.Dom.Css;


namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// Summary description for SvgRadialGradientElement.
	/// </summary>
	public class SvgRadialGradientElement : SvgGradientElement, ISvgRadialGradientElement
	{
		internal SvgRadialGradientElement(string prefix, string localname, string ns, SvgDocument doc)
			: base(prefix, localname, ns, doc)
		{
		}

		#region Implementation of ISvgRadialGradientElement
		private ISvgAnimatedLength cx;
		public ISvgAnimatedLength Cx
		{
			get
			{
				if(!HasAttribute("cx") && ReferencedElement != null)
				{
					return ReferencedElement.Cx;
				}
				else
				{

					if(cx == null)
					{
						cx = new SvgAnimatedLength("cx", GetAttribute("cx"), "50%", this, SvgLengthDirection.Horizontal);
					}
					return cx;
				}
			}
		}

		private ISvgAnimatedLength cy;
		public ISvgAnimatedLength Cy
		{
			get
			{
				if(!HasAttribute("cy") && ReferencedElement != null)
				{
					return ReferencedElement.Cy;
				}
				else
				{

					if(cy == null)
					{
						cy = new SvgAnimatedLength("cy", GetAttribute("cy"), "50%", this, SvgLengthDirection.Vertical);
					}
					return cy;
				}
			}
		}

		private ISvgAnimatedLength r;
		public ISvgAnimatedLength R
		{
			get
			{
				if(!HasAttribute("r") && ReferencedElement != null)
				{
					return ReferencedElement.R;
				}
				else
				{

					if(r == null)
					{
						r = new SvgAnimatedLength("r", GetAttribute("r"), "50%", this, SvgLengthDirection.Viewport);
					}
					return r;
				}
			}
		}

		private ISvgAnimatedLength fx;
		public ISvgAnimatedLength Fx
		{
			get
			{
				if(!HasAttribute("fx") && HasAttribute("fy"))
				{
					return Fy;
				}
				else if(!HasAttribute("fx") && ReferencedElement != null)
				{
					return ReferencedElement.Fx;
				}
				else
				{
					if(fx == null)
					{
						fx = new SvgAnimatedLength("fx", GetAttribute("fx"), "50%", this, SvgLengthDirection.Horizontal);
					}
					return fx;
				}
			}
		}

		private ISvgAnimatedLength fy;
		public ISvgAnimatedLength Fy
		{
			get
			{
				if(!HasAttribute("fy") && HasAttribute("fx"))
				{
					return Fx;
				}
				else if(!HasAttribute("fy") && ReferencedElement != null)
				{
					return ReferencedElement.Fy;
				}
				else
				{
					if(fy == null)
					{
						fy = new SvgAnimatedLength("fy", GetAttribute("fy"), "50%", this, SvgLengthDirection.Vertical);
					}
					return fy;
				}
			}
		}
		#endregion

		#region Implementation of ISvgURIReference
		public new SvgRadialGradientElement ReferencedElement
		{
			get
			{
				return base.ReferencedElement as SvgRadialGradientElement;
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
					case "cx":
						cx = null;
						break;
					case "cy":
						cy = null;
						break;
					case "r":
						r = null;
						break;
					case "fx":
						fx = null;
						break;
					case "fy":
						fy = null;
						break;
				}
			}
		}
		#endregion

	}
}
