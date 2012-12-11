using System;
using System.Xml;

namespace SharpVectors.Dom.Css
{
	/// <summary>
	/// This interface represents a CSS view. The getComputedStyle 
	/// method provides a read only access to the computed values of 
	/// an element. 
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>100</completed>	
	public interface IViewCss /*: Views.IAbstractView*/
	{
		/// <summary>
		/// This method is used to get the computed style as it is defined in [CSS2].
		/// </summary>
		/// <param name="elt">The element whose style is to be computed. This parameter cannot be null.</param>
		/// <param name="pseudoElt">The pseudo-element or null if none</param>
		ICssStyleDeclaration GetComputedStyle(System.Xml.XmlElement elt, string pseudoElt);
	}
}
