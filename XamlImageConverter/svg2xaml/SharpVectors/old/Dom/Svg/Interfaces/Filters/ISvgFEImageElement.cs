using System;

namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// </summary>
	/// <developer>don@donxml.com</developer>
	/// <completed>100</completed>
	public interface ISvgFEImageElement:
		ISvgElement,
		ISvgURIReference,
		ISvgLangSpace,
		ISvgExternalResourcesRequired,
		ISvgFilterPrimitiveStandardAttributes
	{
		ISvgAnimatedPreserveAspectRatio  PreserveAspectRatio{get;}
	}
}
