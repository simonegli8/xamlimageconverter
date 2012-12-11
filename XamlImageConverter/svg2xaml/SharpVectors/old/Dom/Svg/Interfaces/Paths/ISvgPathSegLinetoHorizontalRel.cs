using System;

namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// The SvgPathSegLinetoHorizontalRel interface corresponds to a "relative horizontal lineto" (h) path data command. 
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>100</completed>
	public interface ISvgPathSegLinetoHorizontalRel : ISvgPathSeg
	{
		double X{get;}
	}
}