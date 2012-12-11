using System;

namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// The SvgPathSegLinetoHorizontalAbs interface corresponds to an "absolute horizontal lineto" (H) path data command. 
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>100</completed>
	public interface ISvgPathSegLinetoHorizontalAbs : ISvgPathSeg
	{
		double X{get;}
	}
}