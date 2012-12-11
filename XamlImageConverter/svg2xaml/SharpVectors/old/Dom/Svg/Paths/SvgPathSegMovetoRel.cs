using System;
using System.Windows.Media;
using System.Windows;

namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// The SvgPathSegMovetoRel interface corresponds to an "relative moveto" (m) path data command. 
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>100</completed>
	public class SvgPathSegMovetoRel : SvgPathSegMoveto, ISvgPathSegMovetoRel
	{
		internal SvgPathSegMovetoRel(double x, double y) : base(SvgPathSegType.MoveToRel, x, y)
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
