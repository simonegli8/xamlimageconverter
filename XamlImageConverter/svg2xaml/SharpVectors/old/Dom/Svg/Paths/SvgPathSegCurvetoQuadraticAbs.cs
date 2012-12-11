using System;
using System.Windows.Media;
using System.Text;
using System.Windows;

namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// Summary description for SvgPathSegCurvetoCubicAbs.
	/// </summary>
	public class SvgPathSegCurvetoQuadraticAbs : SvgPathSegCurvetoQuadratic, ISvgPathSegCurvetoQuadraticAbs
	{
		internal SvgPathSegCurvetoQuadraticAbs(double x, double y, double x1, double y1) : base(SvgPathSegType.CurveToQuadraticAbs)
		{
			this.x = x;
			this.y = y;
			this.x1 = x1;
			this.y1 = y1;
		}

		private double x;
		public double X
		{
			get{return x;}
		}		  

		private double y;		
		public double Y
		{
			get{return y;}
		}		  

		private double x1;
		public double X1		
		{
			get{return x1;}
		}		  

		private double y1;		
		public double Y1
		{
			get{return y1;}
		}
		  
		public override Point AbsXY
		{
			get
			{
				return new Point(X, Y);
			}
		}

		public override Point QuadraticX1Y1
		{
			get
			{
				return new Point(X1, Y1);
			}
		}

		/*
		* Convert to cubic bezier using the algorithm from Math:Bezier:Convert in CPAN
		* $p0x+($p1x-$p0x)*2/3
		* $p0y+($p1y-$p0y)*2/3
		* $p1x+($p2x-$p1x)/3
		* $p1x+($p2x-$p1x)/3
		* */

		public override Point CubicX1Y1
		{
			get
			{
				Point prevPoint = PreviousSeg.AbsXY;

				double x1 = prevPoint.X + (X1 - prevPoint.X) * 2/3;
				double y1 = prevPoint.Y + (Y1 - prevPoint.Y) * 2/3;
			
				return new Point(x1, y1);
			}
		}

		public override Point CubicX2Y2
		{
			get
			{
				double x2 = X1 + (X - X1) / 3;
				double y2 = Y1 + (Y - Y1) / 3;

				return new Point(x2, y2);
			}
		}

		public override string PathText
		{
			get
			{
				StringBuilder sb = new StringBuilder();
				sb.Append(PathSegTypeAsLetter);
				sb.Append(X1);
				sb.Append(",");
				sb.Append(Y1);
				sb.Append(",");
				sb.Append(X);
				sb.Append(",");
				sb.Append(Y);

				return sb.ToString();
			}
		}
	}
}
