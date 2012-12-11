using System;
using System.Windows.Media;
using System.Text;
using System.Windows;

namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// The SvgPathSegCurvetoQuadraticSmoothRel interface corresponds to an "relative smooth quadratic curveto" (t) path data command. 
	/// </summary>
	public class SvgPathSegCurvetoQuadraticSmoothRel : SvgPathSegCurvetoQuadratic, ISvgPathSegCurvetoQuadraticSmoothRel
	{
		internal SvgPathSegCurvetoQuadraticSmoothRel(double x, double y) : base(SvgPathSegType.CurveToQuadraticSmoothRel)
		{
			this.x = x;
			this.y = y;
		}

		private double x;
		/// <summary>
		/// The absolute X coordinate for the end point of this path segment. 
		/// </summary>
		public double X
		{
			get{return x;}
		}

		private double y;
		/// <summary>
		/// The absolute Y coordinate for the end point of this path segment. 
		/// </summary>
		public double Y
		{
			get{return y;}
		}

		public override Point QuadraticX1Y1
		{
			get
			{
				SvgPathSeg prevSeg = PreviousSeg;
				if(prevSeg == null || !(prevSeg is SvgPathSegCurvetoQuadratic))
				{
					return prevSeg.AbsXY;
				}
				else
				{
					Point prevXY = prevSeg.AbsXY;
					Point prevX1Y1 = ((SvgPathSegCurvetoQuadratic)prevSeg).QuadraticX1Y1;

					return new Point(2 * prevXY.X - prevX1Y1.X, 2 * prevXY.Y - prevX1Y1.Y);
				}
			}
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

		public override Point CubicX1Y1
		{
			get
			{
				Point prevPoint = PreviousSeg.AbsXY;
				Point x1y1 = QuadraticX1Y1;

				double x1 = prevPoint.X + (x1y1.X - prevPoint.X) * 2/3;
				double y1 = prevPoint.Y + (x1y1.Y - prevPoint.Y) * 2/3;
			
				return new Point(x1, y1);
			}
		}

		public override Point CubicX2Y2
		{
			get
			{
				Point xy = AbsXY;
				Point x1y1 = QuadraticX1Y1;
				double x2 = x1y1.X + (xy.X - x1y1.X) / 3;
				double y2 = x1y1.Y + (xy.Y - x1y1.Y) / 3;

				return new Point(x2, y2);
			}
		}

		public override string PathText
		{
			get
			{
				StringBuilder sb = new StringBuilder();
				sb.Append(PathSegTypeAsLetter);
				sb.Append(X);
				sb.Append(",");
				sb.Append(Y);

				return sb.ToString();
			}
		}
	}
}
