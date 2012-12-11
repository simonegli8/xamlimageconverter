using System;
using System.Windows.Media;

using System.Diagnostics;
using System.Windows;



namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// Summary description for SvgPathSegLinetoAbs.
	/// </summary>

	public class SvgPathSegArcAbs : SvgPathSegArc, ISvgPathSegArcAbs
	{
		internal SvgPathSegArcAbs(double x, double y, double r1, double r2, double angle, bool largeArcFlag, bool sweepFlag) :
			base(SvgPathSegType.ArcAbs, x, y, r1, r2, angle, largeArcFlag, sweepFlag)
		{
			
		}

		public override Point AbsXY
		{
			get
			{
				return new Point(X, Y);
			}
		}
	}
}
