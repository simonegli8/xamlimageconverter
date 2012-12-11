using System;
using System.Xml;
using System.Xml.Serialization;
using SharpVectors.Dom.Stylesheets;

namespace SharpVectors.Dom.Css
{
	public class CssXmlElement : XmlElement, IElementCssInlineStyle
	{
		protected internal CssXmlElement(string prefix, string localname, string ns, CssXmlDocument doc) : base(prefix, localname, ns, doc) 
		{
		}

		public new CssXmlDocument OwnerDocument
		{
			get{
				return (CssXmlDocument)base.OwnerDocument;
			}
		}

		public virtual ICssStyleDeclaration Style
		{
			get
			{
                return new CssStyleDeclaration(GetAttribute("style", String.Empty), null, false, CssStyleSheetType.Inline);
			}
		}

		protected ICssStyleDeclaration _cachedCSD = null;

		public virtual ICssStyleDeclaration GetComputedStyle(string pseudoElt)
		{
			if(_cachedCSD == null)
			{
				CssCollectedStyleDeclaration csd = new CssCollectedStyleDeclaration(this);
				MediaList currentMedia = OwnerDocument.Media;

				if(OwnerDocument.UserAgentStyleSheet != null)
				{
					OwnerDocument.UserAgentStyleSheet.GetStylesForElement(this, pseudoElt, currentMedia, csd);
				}
				((StyleSheetList)OwnerDocument.StyleSheets).GetStylesForElement(this, pseudoElt, csd);

				((CssStyleDeclaration)Style).GetStylesForElement(csd, 0);

				if(OwnerDocument.UserStyleSheet != null)
				{
					OwnerDocument.UserStyleSheet.GetStylesForElement(this, pseudoElt, currentMedia, csd);
				}

				_cachedCSD = csd;
			}
			return _cachedCSD;
		}

		#region Update handling
		public virtual void OnAttributeChange(XmlNodeChangedAction action, XmlAttribute attribute)
		{
			if(AttributeChange != null)
			{
				AttributeChange(action, attribute);
			}
		}

		public virtual event AttributeChangeHandler AttributeChange;

		#endregion
	}

	public delegate void AttributeChangeHandler(XmlNodeChangedAction action, XmlAttribute attribute);
}
