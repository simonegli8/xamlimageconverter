using System;

namespace SharpVectors.Dom.Css
{
	/// <summary>
	/// The Rect interface is used to represent any rect value. This
	/// interface reflects the values in the underlying style 
	/// property. Hence, modifications made to the CSSPrimitiveValue
	/// objects modify the style property. 
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>100</completed>	
	public interface IRect
	{
		/// <summary>
		/// This attribute is used for the left of the rect.
		/// </summary>
		SharpVectors.Dom.Css.ICssPrimitiveValue Left
		{
			get;
		}
	
		/// <summary>
		/// This attribute is used for the bottom of the rect.
		/// </summary>
		SharpVectors.Dom.Css.ICssPrimitiveValue Bottom
		{
			get;
		}
	
		/// <summary>
		/// This attribute is used for the right of the rect.
		/// </summary>
		SharpVectors.Dom.Css.ICssPrimitiveValue Right
		{
			get;
		}
	
		/// <summary>
		/// This attribute is used for the top of the rect.
		/// </summary>
		SharpVectors.Dom.Css.ICssPrimitiveValue Top
		{
			get;
		}
	}
}
