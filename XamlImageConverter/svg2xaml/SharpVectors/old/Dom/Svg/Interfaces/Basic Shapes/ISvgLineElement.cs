namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// The SvgLineElement interface corresponds to the 'line' element. 
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>100</completed>

	public interface ISvgLineElement: 
				ISvgElement,
				ISvgTests,
				ISvgLangSpace,
				ISvgExternalResourcesRequired,
				ISvgStylable,
				ISvgTransformable/*,
				org.w3c.dom.events.IEventTarget*/
	{
		ISvgAnimatedLength X1{get;}

		ISvgAnimatedLength Y1{get;}

		ISvgAnimatedLength X2{get;}
		
		ISvgAnimatedLength Y2{get;}

	}

}
