using System.Windows.Media;

namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// The SvgRectElement interface corresponds to the 'rect' element. 
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>100</completed>
	public interface ISvgImageElement : 
		ISvgElement,
		ISvgTests,
		ISvgStylable,
		ISvgTransformable,
		ISvgLangSpace,
		ISvgExternalResourcesRequired
		/*,
		org.w3c.dom.event.IEventTarget*/
			
	{
		/// <summary>
		/// Corresponds to attribute x on the given 'rect' element.
		/// </summary>
		ISvgAnimatedLength X{get;}

		/// <summary>
		/// Corresponds to attribute y on the given 'rect' element.
		/// </summary>
		ISvgAnimatedLength Y{get;}

		/// <summary>
		/// Corresponds to attribute width on the given 'rect' element.
		/// </summary>
		ISvgAnimatedLength Width{get;}
		
		/// <summary>
		/// Corresponds to attribute height on the given 'rect' element.
		/// </summary>
		ISvgAnimatedLength Height{get;}

		ISvgAnimatedPreserveAspectRatio PreserveAspectRatio{get;}
	}
}
