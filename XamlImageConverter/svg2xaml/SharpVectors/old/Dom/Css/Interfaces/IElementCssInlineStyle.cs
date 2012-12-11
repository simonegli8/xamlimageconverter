using System;

namespace SharpVectors.Dom.Css
{
	/// <summary>
	/// Inline style information attached to elements is exposed 
	/// through the style attribute. This represents the contents of 
	/// the STYLE attribute for HTML elements (or elements in other 
	/// schemas or DTDs which use the STYLE attribute in the same 
	/// way). The expectation is that an instance of the 
	/// ElementCSSInlineStyle interface can be obtained by using 
	/// binding-specific casting methods on an instance of the 
	/// Element interface when the element supports inline CSS 
	/// style informations. 
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>100</completed>	
	
	public interface IElementCssInlineStyle
	{
		/// <summary>
		/// The style attribute
		/// </summary>
		SharpVectors.Dom.Css.ICssStyleDeclaration Style
		{
			get;
		}
	}
}
