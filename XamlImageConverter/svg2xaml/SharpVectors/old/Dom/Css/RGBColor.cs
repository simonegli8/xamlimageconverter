using System;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Diagnostics;
using System.Windows.Media;

namespace SharpVectors.Dom.Css
{
	/// <summary>
	/// The RGBColor interface is used to represent any RGB color value. This interface reflects the values in the underlying style property. Hence, modifications made to the CSSPrimitiveValue objects modify the style property.
	///	A specified RGB color is not clipped (even if the number is outside the range 0-255 or 0%-100%). A computed RGB color is clipped depending on the device.
	/// Even if a style sheet can only contain an integer for a color value, the internal storage of this integer is a double, and this can be used as a double in the specified or the computed style.
	/// A color percentage value can always be converted to a number and vice versa
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>50</completed>
	public class RgbColor : IRgbColor
	{
 		#region Contructors
		/// <summary>
		/// Constructs a RgbColor based on the GDi color
		/// </summary>
		/// <param name="color"></param>
		public RgbColor(Color color)
		{
			_setPrimitiveValues(color);
		}

		/// <summary>
		/// Parses a contructs a RgbColor
		/// </summary>
		/// <param name="str">String to parse to find the color</param>
		public RgbColor(string str)
		{
			str = str.Trim();
			if(str.StartsWith("rgb("))
			{
				str = str.Substring(4, str.Length -5);
				string[] parts = str.Split(',');

				if(parts.Length != 3)
				{
					throw new DomException(DomExceptionType.SyntaxErr);
				}
				else
				{
					try
					{
						string red = parts[0].Trim();
						
						string green = parts[1].Trim();

						string blue = parts[2].Trim();

						_setPrimitiveValues(red, green, blue);
					}
					catch
					{
						throw new DomException(DomExceptionType.SyntaxErr, "rgb() color in the wrong format: " + str);
					}
				}
			}
			else
			{
				str = str.ToLower();
				str = str.Replace("grey", "gray");

				// fix a difference in the GDI+ color table. 
				if(str.Equals("darkseagreen")) str = "#8FBC8F";

				Color gdiColor;
				try
				{
                    gdiColor = (Color) ColorConverter.ConvertFromString(str);
				}
				catch
				{
					gdiColor = Colors.Black;
				}
				_setPrimitiveValues(gdiColor);
			}
		}
		#endregion

		private void _setPrimitiveValues(Color color)
		{
			_red = new CssPrimitiveColorValue(color.R, false);
			_green = new CssPrimitiveColorValue(color.G, false);
			_blue = new CssPrimitiveColorValue(color.B, false);
		}

		private void _setPrimitiveValues(string red, string green, string blue)
		{
			_red = new CssPrimitiveColorValue(red, false);
			_green = new CssPrimitiveColorValue(green, false);
			_blue = new CssPrimitiveColorValue(blue, false);
		}

		#region Public properties
		/// <summary>
		/// A GDI Color representation of the RgbColor
		/// </summary>
		public Color GdiColor
		{
			get
			{
				byte red = Convert.ToByte(Red.GetFloatValue(CssPrimitiveType.Number));
                byte green = Convert.ToByte(Green.GetFloatValue(CssPrimitiveType.Number));
                byte blue = Convert.ToByte(Blue.GetFloatValue(CssPrimitiveType.Number));
				return Color.FromRgb(red, green, blue);
			}
		}

		#endregion
		
		#region Implementation of IRgbColor
		private CssPrimitiveValue _red;
		/// <summary>
		/// This attribute is used for the red value of the RGB color
		/// </summary>
		public ICssPrimitiveValue Red
		{
			get
			{
				return _red;
			}
		}

		private CssPrimitiveValue _green;
		/// <summary>
		/// This attribute is used for the green value of the RGB color.
		/// </summary>
		public ICssPrimitiveValue Green
		{
			get
			{
				return _green;
			}
		}

		private CssPrimitiveValue _blue;
		/// <summary>
		/// This attribute is used for the blue value of the RGB color
		/// </summary>
		public ICssPrimitiveValue Blue
		{
			get
			{
				return _blue;
			}
		}
		#endregion

		public string CssText
		{
			get
			{
				return "rgb(" + ((CssPrimitiveValue)Red).CssText + "," + ((CssPrimitiveValue)Green).CssText + "," + ((CssPrimitiveValue)Blue).CssText + ")";
			}
		}
	}
}
