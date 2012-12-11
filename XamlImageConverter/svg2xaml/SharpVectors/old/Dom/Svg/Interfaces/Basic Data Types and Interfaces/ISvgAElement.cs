using System;

namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// </summary>
	/// <developer>don@donxml.com</developer>
	/// <completed>100</completed>
	public interface ISvgAElement:
		ISvgElement,
		ISvgURIReference,
		ISvgTests,
		ISvgLangSpace,
		ISvgExternalResourcesRequired,
		ISvgStylable,
		ISvgTransformable
	{
		ISvgAnimatedString Target{get;}
	}
}
