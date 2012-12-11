using System;

namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// The SvgPathSegCurvetoCubicSmoothRel interface corresponds to a "relative smooth cubic curveto" (s) path data command.
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>100</completed>
	public interface ISvgPathSegCurvetoCubicSmoothRel : ISvgPathSeg
	{
		double X{get;}
		double Y{get;}
		double X2{get;}
		double Y2{get;}
	}
}