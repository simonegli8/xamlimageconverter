using System;
using System.Windows.Media;
using System.Windows;

namespace SharpVectors.Dom.Svg
{
	public class SvgPathSegMovetoAbs : SvgPathSegMoveto, ISvgPathSegMovetoAbs
	{
		internal SvgPathSegMovetoAbs(double x, double y) : base(SvgPathSegType.MoveToAbs, x, y)
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
