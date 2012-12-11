using System;
using System.Windows.Media;
using System.Text;
using System.Windows;

namespace SharpVectors.Dom.Svg
{
	public class SvgPathSegLinetoVerticalRel : SvgPathSegLineto, ISvgPathSegLinetoVerticalRel
	{
		internal SvgPathSegLinetoVerticalRel(double y) : base(SvgPathSegType.LineToVerticalRel)
		{
			this.y = y;
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
				return new Point(prevPoint.X, prevPoint.Y + Y);
			}
		}

		public override string PathText
		{
			get
			{
				StringBuilder sb = new StringBuilder();
				sb.Append(PathSegTypeAsLetter);
				sb.Append(Y);

				return sb.ToString();
			}
		}
	}
}
