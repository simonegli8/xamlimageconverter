using System;

namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// The SvgPathSegLinetoRel interface corresponds to an "relative lineto" (l) path data command. 
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>100</completed>
	public interface ISvgPathSegLinetoRel : ISvgPathSeg
	{
		double X{get;}
		double Y{get;}
	}
}