using System;

namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// The SvgTextContentElement interface is inherited by various text-related interfaces, such as SvgTextElement, SvgTSpanElement, SvgTRefElement, SvgAltGlyphElement and SvgTextPathElement. 
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>10</completed>
	public interface ISvgTextContentElement	:
				ISvgElement,
				ISvgTests,
				ISvgLangSpace,
				ISvgExternalResourcesRequired,
				ISvgStylable/*,
				org.w3c.dom.events.EventTarget*/
	{
		ISvgAnimatedLength TextLength{get;}
		ISvgAnimatedEnumeration LengthAdjust{get;}
		double GetNumberOfChars();
		double GetComputedTextLength();
		double GetSubStringLength(double charnum,double nchars);
		ISvgPoint GetStartPositionOfChar(double charnum);
		ISvgPoint GetEndPositionOfChar(double charnum);
		ISvgRect GetExtentOfChar(double charnum);
		double GetRotationOfChar(double charnum);
		double GetCharNumAtPosition(ISvgPoint point);
		void SelectSubString(double charnum,double nchars);

	}
}
