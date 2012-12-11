using SharpVectors.Dom.Css;

namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// 
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>10</completed>
	public interface ISvgStylable  
	{
		ISvgAnimatedString ClassName{get;}
		ICssStyleDeclaration Style{get;}
		ICssValue GetPresentationAttribute(string name);
		// org.w3c.dom.css.CssStyleDeclaration Style{get;}	
		// org.w3c.dom.css.CSSValue getPresentationAttribute(string name);
	}
}
