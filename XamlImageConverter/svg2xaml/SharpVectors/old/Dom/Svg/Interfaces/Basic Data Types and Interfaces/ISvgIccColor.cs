using System;

namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// 
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>100</completed>

	public interface ISvgIccColor
	{
		/// <summary>
		/// The list of color values that define this ICC color. 
		/// Each color value is an arbitrary floating point number. 
		/// </summary>
		SharpVectors.Dom.Svg.ISvgNumberList Colors
		{
			get;
		}
		
		/// <summary>
		/// The name of the color profile, which is the first 
		/// parameter of an ICC color specification.  Inheriting
		/// class shouls throw exception on setting a read only value 
		/// </summary>
		string ColorProfile
		{
			get;
			set;
		}
	}
}
