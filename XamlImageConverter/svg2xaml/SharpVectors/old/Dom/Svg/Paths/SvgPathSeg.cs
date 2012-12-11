using System;
using System.Windows.Media;
using System.Windows;



namespace SharpVectors.Dom.Svg
{
	public abstract class SvgPathSeg : ISvgPathSeg
	{
		#region Constructors
		protected SvgPathSeg(SvgPathSegType type)
		{
			this.type = type;
		}
		#endregion

		protected SvgPathSegType type;

		internal void setList(SvgPathSegList list)
		{
            this.list = list;
		}

		internal void setIndex(int index)
		{
			this.index = index;
		}

		internal void setIndexWithDiff(int diff)
		{
			this.index += diff;
		}

		protected SvgPathSegList list;
		protected int index;

		#region Implementation of ISvgPathSeg
		public SvgPathSegType PathSegType
		{
			get{return type;}
		}

		public string PathSegTypeAsLetter	
		{
			get
			{
				switch(type)
				{
					case SvgPathSegType.ArcAbs:
						return "A";
					case SvgPathSegType.ArcRel:
						return "a";
					case SvgPathSegType.ClosePath:
						return "z";
					case SvgPathSegType.CurveToCubicAbs:
						return "C";
					case SvgPathSegType.CurveToCubicRel:
						return "c";
					case SvgPathSegType.CurveToCubicSmoothAbs:
						return "S";
					case SvgPathSegType.CurveToCubicSmoothRel:
						return "s";
					case SvgPathSegType.CurveToQuadraticAbs:
						return "Q";
					case SvgPathSegType.CurveToQuadraticRel:
						return "q";
					case SvgPathSegType.CurveToQuadraticSmoothAbs:
						return "T";
					case SvgPathSegType.CurveToQuadraticSmoothRel:
						return "t";
					case SvgPathSegType.LineToAbs:
						return "L";
					case SvgPathSegType.LineToHorizontalAbs:
						return "H";
					case SvgPathSegType.LineToHorizontalRel:
						return "h";
					case SvgPathSegType.LineToRel:
						return "l";
					case SvgPathSegType.LineToVerticalAbs:
						return "V";
					case SvgPathSegType.LineToVerticalRel:
						return "v";
					case SvgPathSegType.MoveToAbs:
						return "M";
					case SvgPathSegType.MoveToRel:
						return "m";
					default:
						return String.Empty;
				}
			}
		}
		#endregion

		#region Public members
		public abstract string PathText{get;}
		public abstract Point AbsXY{get;}
		public abstract double StartAngle{get;}
		public abstract double EndAngle{get;}
		public SvgPathSeg PreviousSeg
		{
			get
			{
				return list.GetPreviousSegment(this);
			}
		}
		public SvgPathSeg NextSeg
		{
			get
			{
				return list.GetNextSegment(this);
			}
		}
		public int Index
		{
			get
			{
				return index;
			}
		}

		public virtual double Length
		{
			get
			{
				return 0;
			}
		}
		#endregion
	}
}
