using System;
using System.Windows.Media;
using System.Text;
using System.Windows;

namespace SharpVectors.Dom.Svg
{
	public class SvgPathSegLinetoAbs : SvgPathSegLineto, ISvgPathSegLinetoAbs
	{
		internal SvgPathSegLinetoAbs(double x, double y) : base(SvgPathSegType.LineToAbs)
		{
			this.x = x;
			this.y = y;
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
		
		public override Point AbsXY
		{
			get
			{
				return new Point(X, Y);
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
