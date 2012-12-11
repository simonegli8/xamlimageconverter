using System;
using System.Windows.Media;
using System.Text;
using System.Windows;

namespace SharpVectors.Dom.Svg
{
	public abstract class SvgPathSegLineto : SvgPathSeg
	{
		protected SvgPathSegLineto(SvgPathSegType type) : base(type)
		{
		}

		public abstract override Point AbsXY{get;}

		private Point getPrevPoint()
		{
			SvgPathSeg prevSeg = PreviousSeg;
			Point prevPoint;
			if(prevSeg == null)
			{
				prevPoint = new Point(0,0);
			}
			else
			{
				prevPoint = prevSeg.AbsXY;
			}
			return prevPoint;
		}
		public override double StartAngle
		{
			get
			{
				Point prevPoint = getPrevPoint();
				Point curPoint = AbsXY;

				double dx = curPoint.X - prevPoint.X;
				double dy = curPoint.Y - prevPoint.Y;

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
				double a = StartAngle;
				a += 180;
				a %= 360;
				return a;
			}
		}

		public override double Length
		{
			get
			{
				Point prevPoint = getPrevPoint();
				Point thisPoint = AbsXY;

				double dx = thisPoint.X - prevPoint.X;
				double dy = thisPoint.Y - prevPoint.Y;

				return (double)Math.Sqrt(dx*dx+dy*dy);
			}
		}
	}
}
