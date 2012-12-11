using System;
using System.Windows.Media;

using System.Text;
using System.Windows;


namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// Summary description for SvgPathSegCurvetoCubicAbs.
	/// </summary>
	public class SvgPathSegCurvetoQuadraticRel : SvgPathSegCurvetoQuadratic, ISvgPathSegCurvetoQuadraticRel
	{
		internal SvgPathSegCurvetoQuadraticRel(double x, double y, double x1, double y1) : base(SvgPathSegType.CurveToQuadraticRel)
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
				SvgPathSeg prevSeg = PreviousSeg;
				Point prevPoint;
				if(prevSeg == null) prevPoint = new Point(0,0);
				else prevPoint = prevSeg.AbsXY;

				return new Point(prevPoint.X + X, prevPoint.Y + Y);
			}
		}

		public override Point QuadraticX1Y1
		{
			get
			{
				SvgPathSeg prevSeg = PreviousSeg;
				Point prevPoint;
				if(prevSeg == null) prevPoint = new Point(0,0);
				else prevPoint = prevSeg.AbsXY;

				return new Point(prevPoint.X + X1, prevPoint.Y + Y1);
			}
		}

		public override Point CubicX1Y1
		{
			get
			{
				Point prevPoint = PreviousSeg.AbsXY;

				double x1 = prevPoint.X + X1 * 2/3;
				double y1 = prevPoint.Y + Y1 * 2/3;
			
				return new Point(x1, y1);
			}
		}

		public override Point CubicX2Y2
		{
			get
			{
				Point prevPoint = PreviousSeg.AbsXY;

				double x2 = X1 + prevPoint.X + (X - X1) / 3;
				double y2 = Y1 + prevPoint.Y + (Y - Y1) / 3;

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
