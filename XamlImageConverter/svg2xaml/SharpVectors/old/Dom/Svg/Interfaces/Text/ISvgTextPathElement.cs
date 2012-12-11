namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// The SvgTextPathElement interface corresponds to the 'textPath' element. 
	/// </summary>
	/// <developer></developer>
	/// <completed>0</completed>
	public interface ISvgTextPathElement	:
				ISvgURIReference,
				ISvgTextContentElement
	{
		ISvgAnimatedLength StartOffset{get;}
		ISvgAnimatedEnumeration Method{get;}
		ISvgAnimatedEnumeration Spacing{get;}

	}
}
