using System;



namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// The SvgTSpanElement interface corresponds to the 'tspan' element. 
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>100</completed>
	public class SvgTSpanElement : SvgTextPositioningElement, ISvgTSpanElement
	{
		internal SvgTSpanElement(string prefix, string localname, string ns, SvgDocument doc) : base(prefix, localname, ns, doc) 
		{
		}
	}
}
