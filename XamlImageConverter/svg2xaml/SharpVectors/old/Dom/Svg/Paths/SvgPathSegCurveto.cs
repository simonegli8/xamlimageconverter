using System;
using System.Windows.Media;
using System.Windows;

namespace SharpVectors.Dom.Svg
{
	public abstract class SvgPathSegCurveto : SvgPathSeg
	{	
		protected SvgPathSegCurveto(SvgPathSegType type) : base(type)
		{
		}

		public abstract override Point AbsXY{get;}
		public abstract Point CubicX1Y1{get;}
		public abstract Point CubicX2Y2{get;}

		public override double StartAngle
		{
			get
			{
				Point p1 = PreviousSeg.AbsXY;
				Point p2 = CubicX1Y1;

				double dx = p2.X - p1.X;
				double dy = p2.Y - p1.Y;
				double a = (double)(Math.Atan2(dy, dx) * 180 / Math.PI);
				a += 270;
				a %= 360;
				return a;
			}
		}

		public override double EndAngle
		{
			get
			{
				Point p1 = CubicX2Y2;
				Point p2 = AbsXY;

				double dx = p1.X - p2.X;
				double dy = p1.Y - p2.Y;
				double a = (double)(Math.Atan2(dy, dx) * 180 / Math.PI);
				a += 270;
				a %= 360;
				return a;
			}
		}
	
	}
}
