using System;
using System.Windows.Media;
using System.Text;
using System.Windows;

namespace SharpVectors.Dom.Svg
{
	public class SvgPathSegCurvetoCubicAbs : SvgPathSegCurvetoCubic, ISvgPathSegCurvetoCubicAbs
	{
		internal SvgPathSegCurvetoCubicAbs(double x, double y, double x1, double y1, double x2, double y2) : base(SvgPathSegType.CurveToCubicAbs)
		{
			this.x = x;
			this.y = y;
			this.x1 = x1;
			this.y1 = y1;
			this.x2 = x2;
			this.y2 = y2;
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

		private double x2;
		public double X2		
		{
			get{return x2;}
		}		  

		private double y2;		
		public double Y2
		{
			get{return y2;}
		}
	  
		public override Point AbsXY
		{
			get
			{
				return new Point(X, Y);
			}
		}
		public override Point CubicX1Y1
		{
			get
			{
				return new Point(X1, Y1);
			}
		}
		public override Point CubicX2Y2
		{
			get
			{
				return new Point(X2, Y2);
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
				sb.Append(X2);
				sb.Append(",");
				sb.Append(Y2);
				sb.Append(",");
				sb.Append(X);
				sb.Append(",");
				sb.Append(Y);

				return sb.ToString();
			}
		}
	}
}
