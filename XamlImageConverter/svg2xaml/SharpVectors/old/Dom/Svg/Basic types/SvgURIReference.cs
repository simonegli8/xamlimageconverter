using System;
using System.Xml;

using SharpVectors.Dom.Css;

namespace SharpVectors.Dom.Svg
{
	public class SvgURIReference : ISvgURIReference
	{
		public SvgURIReference(SvgElement ownerElement)
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
			if(attribute.NamespaceURI == SvgDocument.XLinkNamespace &&
				attribute.LocalName == "href")
			{
				href = null;
			}
		}
		#endregion

		#region Implementation of ISvgURIReference
		private ISvgAnimatedString href;
		public ISvgAnimatedString Href
		{
			get
			{
				if(href == null)
				{
					href = new SvgAnimatedString(ownerElement.GetAttribute("href", SvgDocument.XLinkNamespace));
				}
				return href;
			}
		}
		#endregion

		private XmlNode referencedNode;
		public XmlNode ReferencedNode
		{
			get
			{
				if(referencedNode == null)
				{
					if(ownerElement.HasAttribute("href", SvgDocument.XLinkNamespace))
					{
						//Uri absUri =  new Uri(new Uri(ownerElement.BaseURI), Href.AnimVal);
                        Uri absUri = ownerElement.OwnerDocument.ResolveUri(ownerElement.BaseURI, Href.AnimVal);
						referencedNode = ownerElement.OwnerDocument.GetNodeByUri(absUri);
					}
					else
					{
						referencedNode = null; 
					}
				}
				return referencedNode;
			}
		}
	}
}
