namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// Interface SvgURIReference defines an interface which applies to all elements which have the collection of XLink attributes, such as xlink:href, which define a URI reference. 
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>25</completed>
	public interface ISvgURIReference
	{
		ISvgAnimatedString Href{get;}
	}
}
