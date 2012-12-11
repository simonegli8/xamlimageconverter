using System;

namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// The SvgPathSegMovetoRel interface corresponds to an "relative moveto" (m) path data command. 
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>100</completed>
	public interface ISvgPathSegMovetoRel : ISvgPathSeg
	{
		double X{get;}
		double Y{get;}
	}
}