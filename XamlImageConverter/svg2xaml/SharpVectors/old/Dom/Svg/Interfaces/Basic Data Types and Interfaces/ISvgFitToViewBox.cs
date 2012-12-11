namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// Interface SvgFitToViewBox defines DOM attributes that apply to 
	/// elements which have XML attributes viewBox and 
	/// preserveAspectRatio.
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>80</completed>
	public interface ISvgFitToViewBox
	{
		ISvgAnimatedRect ViewBox{get;}
		ISvgAnimatedPreserveAspectRatio PreserveAspectRatio{get;}
	}
}