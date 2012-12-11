using System;

namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// The SvgPathSegCurvetoCubicRel interface corresponds to a "relative cubic Bézier curveto" (c) path data command. 
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>100</completed>
	public interface ISvgPathSegCurvetoCubicRel : ISvgPathSeg
	{
		double X{get;}
		double Y{get;}
		double X1{get;}
		double Y1{get;}
		double X2{get;}
		double Y2{get;}
	}
}