using System;


namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// The SvgDescElement interface corresponds to the 'desc' element. 
	/// </summary>
	public class SvgDescElement : SvgStyleableElement, ISvgDescElement
	{
		internal SvgDescElement(string prefix, string localname, string ns, SvgDocument doc) : base(prefix, localname, ns, doc) 
		{
		}
	}
}
