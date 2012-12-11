namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// The SvgEllipseElement interface corresponds to the 'ellipse' element. 
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>100</completed>
	public interface ISvgEllipseElement	:
		ISvgElement,
		ISvgTests,
		ISvgLangSpace,
		ISvgExternalResourcesRequired,
		ISvgStylable,
		ISvgTransformable/*,
		org.w3c.dom.events.EventTarget*/
	{

		ISvgAnimatedLength Cx{get;}

		ISvgAnimatedLength Cy{get;}

		ISvgAnimatedLength Rx{get;}
		
		ISvgAnimatedLength Ry{get;}

	}
}
