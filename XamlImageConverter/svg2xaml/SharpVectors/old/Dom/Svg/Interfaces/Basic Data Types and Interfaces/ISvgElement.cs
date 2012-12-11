using System;
using SharpVectors.Dom;

namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// All of the Svg DOM interfaces that correspond directly to 
	/// elements in the Svg language (e.g., the SvgPathElement 
	/// interface corresponds directly to the 'path' element in the 
	/// language) are derivative from base class SvgElement. 
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>100</completed>
	public interface ISvgElement : IXmlElement
	{
		/// <summary>
		/// The value of the id attribute on the given element.
		/// Inheriting class should throw an exception when trying
		/// to update a read only element
		/// </summary>
		string Id{get;set;}

		// Disabled since .NET already provide BaseUri on XmlNode

		/*/// <summary>
		/// Corresponds to attribute xml:base on the given element.
		/// Inheriting class should throw an exception when trying
		/// to update a read only element
		/// </summary>
		string Xmlbase{get;set;}
		*/

		/// <summary>
		/// The nearest ancestor 'svg' element. Null if the given 
		/// element is the outermost 'svg' element.
		/// </summary>
		ISvgSvgElement OwnerSvgElement{get;}

		/// <summary>
		///     The element which established the current viewport. 
		///     Often, the nearest ancestor 'svg' element. Null if 
		///     the given element is the outermost 'svg' element.
		/// </summary>
		ISvgElement ViewportElement{get;}
	}
}

