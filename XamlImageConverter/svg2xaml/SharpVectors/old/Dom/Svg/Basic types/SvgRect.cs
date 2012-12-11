using System;
using System.Text.RegularExpressions;

namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// Rectangles are defined as consisting of a (x,y) coordinate pair 
	/// identifying a minimum X value, a minimum Y value, and a width 
	/// and height, which are usually constrained to be non-negative. 
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <developer>kevin@kevlindev.com</developer>
	/// <completed>100</completed>
	public class SvgRect : ISvgRect
	{
        #region Static Fields
        public static readonly SvgRect Empty = new SvgRect(0,0,0,0);
        #endregion

        #region Fields
        private double x;
        private double y;
        private double width;
        private double height;
		#endregion

		#region Constructors
        public SvgRect(double x, double y, double width, double height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

		public SvgRect(string str)
		{
			string replacedStr = Regex.Replace(str, @"(\s|,)+", ",");
			string[] tokens = replacedStr.Split(new char[]{','});
			if(tokens.Length == 4)
			{
				this.x = SvgNumber.ParseToFloat(tokens[0]);
				this.y = SvgNumber.ParseToFloat(tokens[1]);
				this.width = SvgNumber.ParseToFloat(tokens[2]);
				this.height = SvgNumber.ParseToFloat(tokens[3]);
			}
			else
			{
				throw new SvgException(SvgExceptionType.SvgInvalidValueErr, "Invalid SvgRect value: " + str);
			}
		}
        /*
		public SvgRect(Rect initRect)
		{
			gdiRect = initRect;
		}
        */
		#endregion

		#region Public properties
		public bool IsEmpty
		{
			get
			{
				return ( width <= 0 || height <= 0 );
			}
		}
		#endregion

        #region ISvgRect Interface
		public double X
		{
			get
			{
				return x;
			}
			set
			{
				x = value;
			}
		}
		public double Y
		{
			get
			{
				return y;
			}
			set
			{
				y = value;
			}
		}
		public double Width
		{
			get
			{
				return width;
			}
			set
			{
				width = value;
			}
		}
		public double Height
		{
			get
			{
				return height;
			}
			set
			{
				height = value;
			}
		}
		#endregion
	}
}
