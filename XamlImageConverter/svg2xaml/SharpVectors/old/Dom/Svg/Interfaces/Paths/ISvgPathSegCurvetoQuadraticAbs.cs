using System;

namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// The SvgPathSegCurvetoQuadraticAbs interface corresponds to an "absolute quadratic Bézier curveto" (Q) path data command. 
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>100</completed>
	public interface ISvgPathSegCurvetoQuadraticAbs : ISvgPathSeg
	{
		double X{get;}
		double Y{get;}
		double X1{get;}
		double Y1{get;}
	}
}