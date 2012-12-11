using System;
using System.Windows.Media;
using System.Text;
using System.Windows;

namespace SharpVectors.Dom.Svg
{
	public class SvgPathSegLinetoHorizontalAbs : SvgPathSegLineto, ISvgPathSegLinetoHorizontalAbs
	{
		internal SvgPathSegLinetoHorizontalAbs(double x) : base(SvgPathSegType.LineToHorizontalAbs)
		{
			this.x = x;
		}

		private double x;
		public double X
		{
			get{return x;}
		}

		public override Point AbsXY
		{
			get
			{
				SvgPathSeg prevSeg = PreviousSeg;
				Point prevPoint;
				if(prevSeg == null) prevPoint = new Point(0,0);
				else prevPoint = prevSeg.AbsXY;
				return new Point(X, prevPoint.Y);
			}
		}

		public override string PathText
		{
			get
			{
				StringBuilder sb = new StringBuilder();
				sb.Append(PathSegTypeAsLetter);
				sb.Append(X);

				return sb.ToString();
			}
		}
	}
}
