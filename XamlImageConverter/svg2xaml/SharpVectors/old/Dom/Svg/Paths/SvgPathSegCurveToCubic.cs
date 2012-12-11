using System;
using System.Windows.Media;
using System.Windows;

namespace SharpVectors.Dom.Svg
{
	public abstract class SvgPathSegCurvetoCubic : SvgPathSegCurveto
	{
		protected SvgPathSegCurvetoCubic(SvgPathSegType type) : base(type)
		{
		}

		public abstract override Point AbsXY{get;}
		public abstract override Point CubicX1Y1{get;}
		public abstract override Point CubicX2Y2{get;}
	}
}
