using System;

namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// The SvgPathSegArcRel interface corresponds to a "relative arcto" (a) path data command. 
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>100</completed>
	public interface ISvgPathSegArcRel : ISvgPathSeg 
	{
		double X{get;}
		double Y{get;}
		double R1{get;}
		double R2{get;}
		double Angle{get;}
		bool LargeArcFlag{get;}
		bool SweepFlag{get;}
	}
}