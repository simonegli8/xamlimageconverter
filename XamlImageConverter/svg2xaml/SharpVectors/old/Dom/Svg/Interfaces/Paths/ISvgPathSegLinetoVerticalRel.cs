using System;
namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// The SvgPathSegLinetoVerticalRel interface corresponds to a "relative vertical lineto" (v) path data command. 
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>100</completed>
	public interface ISvgPathSegLinetoVerticalRel : ISvgPathSeg
	{
		double Y{get;}
	}
}