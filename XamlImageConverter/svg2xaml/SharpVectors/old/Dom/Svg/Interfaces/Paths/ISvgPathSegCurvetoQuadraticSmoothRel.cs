using System;

namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// The SvgPathSegCurvetoQuadraticSmoothRel interface corresponds to a "relative smooth quadratic curveto" (t) path data command. 
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>100</completed>
	public interface ISvgPathSegCurvetoQuadraticSmoothRel : ISvgPathSeg
	{
		double X{get;}
		double Y{get;}
	}
}