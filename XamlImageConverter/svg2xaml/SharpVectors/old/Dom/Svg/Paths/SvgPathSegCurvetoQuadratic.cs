using System;
using System.Windows.Media;
using System.Windows;

namespace SharpVectors.Dom.Svg
{
	public abstract class SvgPathSegCurvetoQuadratic : SvgPathSegCurveto
	{
		protected SvgPathSegCurvetoQuadratic(SvgPathSegType type) : base(type)
		{
		}
		public abstract override Point AbsXY{get;}
		public abstract override Point CubicX1Y1{get;}
		public abstract override Point CubicX2Y2{get;}

		public abstract Point QuadraticX1Y1{get;}
	}
}
