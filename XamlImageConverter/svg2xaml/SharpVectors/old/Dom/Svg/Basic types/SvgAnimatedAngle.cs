using System;

namespace SharpVectors.Dom.Svg
{
	public class SvgAnimatedAngle : ISvgAnimatedAngle
	{
        #region Fields
        private ISvgAngle baseVal;
        private ISvgAngle animVal;
        #endregion

        #region Constructor
		public SvgAnimatedAngle(string s, string defaultValue)
		{
			animVal = baseVal = new SvgAngle(s, defaultValue, false);
		}
        #endregion

		#region Implementation of ISvgAnimatedAngle
		public ISvgAngle BaseVal
		{
			get
			{
				return baseVal;
			}
		}
		
		public ISvgAngle AnimVal
		{
			get
			{
				return animVal;
			}
		}
		#endregion
	}
}