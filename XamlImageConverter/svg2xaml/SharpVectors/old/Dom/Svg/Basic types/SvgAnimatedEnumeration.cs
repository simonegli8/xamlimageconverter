using System;

namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// Summary description for SvgAnimatedEnumeration.
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <developer>kevin@kevlindev.com</developer>
	/// <completed>100</completed>
	public class SvgAnimatedEnumeration : ISvgAnimatedEnumeration
	{
		#region Private Fields
		private Enum baseVal;
		private Enum animVal;
		#endregion

		#region Constructor
		public SvgAnimatedEnumeration(Enum val)
		{
			baseVal = animVal = val;
		}
		#endregion

        #region ISvgAnimatedEnumeration Interface
		public Enum BaseVal
		{
			get
			{
				return baseVal;
			}
			set
			{
				baseVal = value;
			}
		}

		public Enum AnimVal
		{
			get
			{
				return animVal;
			}
		}
		#endregion
	}
}
