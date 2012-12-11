using System;

namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// 
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <developer>kevin@kevlindev.com</developer>
	/// <completed>100</completed>
	public class SvgAnimatedRect : ISvgAnimatedRect
	{
		#region Private Fields
		private SvgRect baseVal;
		private SvgRect animVal;
		#endregion

		#region Constructors
		public SvgAnimatedRect(string str)
		{
			baseVal = new SvgRect(str);
			animVal = baseVal;
		}

		public SvgAnimatedRect(SvgRect rect)
		{
			baseVal = rect;
			animVal = baseVal;
		}
		#endregion

        #region ISvgAnimatedRect Interface
		public ISvgRect BaseVal
		{
			get
			{
				return baseVal;
			}
		}
		
		public ISvgRect AnimVal
		{
			get
			{
				return animVal;
			}
		}
		#endregion
	}
}