using System;
using System.Windows.Media;

using System.Diagnostics;
using System.Windows;



namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// Summary description for SvgPathSegLinetoAbs.
	/// </summary>
	public class SvgPathSegArcRel : SvgPathSegArc, ISvgPathSegArcRel
	{
		internal SvgPathSegArcRel(double x, double y, double r1, double r2, double angle, bool largeArcFlag, bool sweepFlag) :
		base(SvgPathSegType.ArcRel, x, y, r1, r2, angle, largeArcFlag, sweepFlag)
		{
		}

		public override Point AbsXY
		{
			get
			{
				SvgPathSeg prevSeg = PreviousSeg;
				Point prevPoint;
				if(prevSeg == null) prevPoint = new Point(0,0);
				else prevPoint = prevSeg.AbsXY;
				return new Point(prevPoint.X + X, prevPoint.Y + Y);
			}
		}
	}
}
