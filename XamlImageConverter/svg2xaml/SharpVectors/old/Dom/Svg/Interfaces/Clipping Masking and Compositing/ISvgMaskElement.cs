namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// Used by SvgMaskElement.
	/// </summary>
	/// <developer>tabascopete78@yahoo.com</developer>
	/// <completed>10</completed>
	public interface ISvgMaskElement :
		ISvgElement,
		ISvgTests,
		ISvgLangSpace,
		ISvgExternalResourcesRequired,
		ISvgStylable
	{
		ISvgAnimatedEnumeration MaskUnits {get;}
		ISvgAnimatedEnumeration MaskContentUnits {get;}
		ISvgAnimatedLength X {get;}
		ISvgAnimatedLength Y {get;}
		ISvgAnimatedLength Width {get;}
		ISvgAnimatedLength Height {get;}	
	}
}
