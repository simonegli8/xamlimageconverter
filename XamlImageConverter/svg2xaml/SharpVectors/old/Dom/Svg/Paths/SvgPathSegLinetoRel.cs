using System;
using System.Windows.Media;
using System.Text;
using System.Windows;

namespace SharpVectors.Dom.Svg
{
	public class SvgPathSegLinetoRel : SvgPathSegLineto, ISvgPathSegLinetoRel
	{
		internal SvgPathSegLinetoRel(double x, double y) : base(SvgPathSegType.LineToRel)
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
				SvgPathSeg prevSeg = PreviousSeg;
				Point prevPoint;
				if(prevSeg == null) prevPoint = new Point(0,0);
				else prevPoint = prevSeg.AbsXY;
				return new Point(prevPoint.X + X, prevPoint.Y + Y);
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
