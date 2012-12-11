using System;
using System.Xml;
using SharpVectors.Dom.Css;

namespace SharpVectors.Dom.Svg
{

	public class SvgLinearGradientElement : SvgGradientElement, ISvgLinearGradientElement
	{
		internal SvgLinearGradientElement(string prefix, string localname, string ns, SvgDocument doc)
			: base(prefix, localname, ns, doc)
		{
		}

		#region Implementation of ISvgLinearGradientElement
		private ISvgAnimatedLength x1;
		public ISvgAnimatedLength X1
		{
			get
			{
				if(!HasAttribute("x1") && ReferencedElement != null)
				{
					return ReferencedElement.X1;
				}
				else
				{

					if(x1 == null)
					{
						x1 = new SvgAnimatedLength("x1", GetAttribute("x1"), "0%", this, SvgLengthDirection.Horizontal);
					}
					return x1;
				}
			}
		}

		private ISvgAnimatedLength y1;
		public ISvgAnimatedLength Y1
		{
			get
			{
				if(!HasAttribute("y1") && ReferencedElement != null)
				{
					return ReferencedElement.Y1;
				}
				else
				{

					if(y1 == null)
					{
						y1 = new SvgAnimatedLength("y1", GetAttribute("y1"), "0%", this, SvgLengthDirection.Vertical);
					}
					return y1;
				}
			}
		}

		private ISvgAnimatedLength x2;
		public ISvgAnimatedLength X2
		{
			get
			{
				if(!HasAttribute("x2") && ReferencedElement != null)
				{
					return ReferencedElement.X2;
				}
				else
				{

					if(x2 == null)
					{
						x2 = new SvgAnimatedLength("x2", GetAttribute("x2"), "100%", this, SvgLengthDirection.Horizontal);
					}
					return x2;
				}
			}
		}

		private ISvgAnimatedLength y2;
		public ISvgAnimatedLength Y2
		{
			get
			{
				if(!HasAttribute("y2") && ReferencedElement != null)
				{
					return ReferencedElement.Y2;
				}
				else
				{

					if(y2 == null)
					{
						y2 = new SvgAnimatedLength("y2", GetAttribute("y2"), "0%", this, SvgLengthDirection.Vertical);
					}
					return y2;
				}
			}
		}
		#endregion

		#region Implementation of ISvgURIReference
		public new SvgLinearGradientElement ReferencedElement
		{
			get
			{
				return base.ReferencedElement as SvgLinearGradientElement;
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
					case "x1":
						x1 = null;
						break;
					case "y1":
						y1 = null;
						break;
					case "x2":
						x2 = null;
						break;
					case "y2":
						y2 = null;
						break;
				}
			}
		}
		#endregion

	}
}
