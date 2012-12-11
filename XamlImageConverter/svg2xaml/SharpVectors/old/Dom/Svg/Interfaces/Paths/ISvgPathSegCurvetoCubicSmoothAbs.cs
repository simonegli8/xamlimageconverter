using System;

namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// The SvgPathSegCurvetoCubicSmoothAbs interface corresponds to an "absolute smooth cubic curveto" (S) path data command. 
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>100</completed>
	public interface ISvgPathSegCurvetoCubicSmoothAbs : ISvgPathSeg
	{
		double X{get;}
		double Y{get;}
		double X2{get;}
		double Y2{get;}
	}
}