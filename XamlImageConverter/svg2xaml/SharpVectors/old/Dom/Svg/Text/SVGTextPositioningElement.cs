using System;



namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// The SvgTextPositioningElement interface is inherited by text-related interfaces: SvgTextElement, SvgTSpanElement, SvgTRefElement and SvgAltGlyphElement. 
	/// </summary>
	public class SvgTextPositioningElement : SvgTextContentElement, ISvgTextPositioningElement
	{
		internal SvgTextPositioningElement(string prefix, string localname, string ns, SvgDocument doc) : base(prefix, localname, ns, doc) 
		{
		}

		public ISvgAnimatedLengthList X
		{
			get
			{
				return (ISvgAnimatedLengthList)new SvgAnimatedLengthList("x", GetAttribute("x"), this, SvgLengthDirection.Horizontal);
			}
		}

		public ISvgAnimatedLengthList Y
		{
			get
			{
				return (ISvgAnimatedLengthList)new SvgAnimatedLengthList("y", GetAttribute("y"), this, SvgLengthDirection.Vertical);
			}
		}

		public ISvgAnimatedLengthList Dx
		{
			get
			{
				return (ISvgAnimatedLengthList)new SvgAnimatedLengthList("dx", GetAttribute("dx"), this, SvgLengthDirection.Horizontal);
			}
		}

		public ISvgAnimatedLengthList Dy
		{
			get
			{
				return (ISvgAnimatedLengthList)new SvgAnimatedLengthList("dy", GetAttribute("dy"), this, SvgLengthDirection.Vertical);
			}
		}

		public ISvgAnimatedNumberList Rotate
		{
			get
			{
				return (ISvgAnimatedNumberList)new SvgAnimatedNumberList(GetAttribute("rotate"));
			}
		}
	}
}
