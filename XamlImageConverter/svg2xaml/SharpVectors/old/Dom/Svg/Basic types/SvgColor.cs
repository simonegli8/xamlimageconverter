using System;
using System.Windows.Media;
using SharpVectors.Dom.Css;

namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// Summary description for SvgColor.
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>100</completed>
	public class SvgColor : CssValue, ISvgColor
	{
		#region Private Level Fields
		protected RgbColor _rgbColor;
		#endregion

		#region Constructors
		protected SvgColor() : base(CssValueType.PrimitiveValue, String.Empty, false)
		{
		}

		public SvgColor(string str) : base(CssValueType.PrimitiveValue, str, false)
		{
			_parseColor(str);
		}
		#endregion

		protected void _parseColor(string str)
		{
			str = str.Trim();
			if(str.Equals("currentColor"))
			{
				SetColor(SvgColorType.CurrentColor, null, null);
			}
			else if(str.IndexOf("icc-color(")>-1)
			{
				int iccStart = str.IndexOf("icc-color(");
				string strRgb = str.Substring(0, iccStart).Trim();
				string strIcc = str.Substring(iccStart);

				SetColor(SvgColorType.RgbColorIccColor, strRgb, strIcc);
			}
			else
			{
				SetColor(SvgColorType.RgbColor, str, String.Empty);
			}
		}

		#region Public Properties


		public override string CssText
		{
			get
			{
				string ret;
				switch(ColorType)
				{
					case SvgColorType.RgbColor:
						ret = _rgbColor.CssText;
						break;
					case SvgColorType.RgbColorIccColor:
						ret = _rgbColor.CssText;
						break;
					case SvgColorType.CurrentColor:
						ret = "currentColor";
						break;
					default:
						ret = String.Empty;
						break;
				}
				return ret;
			}
			set
			{
				base.CssText = value;
                _parseColor(value);
			}
		}

		private SvgColorType _colorType;
		public SvgColorType ColorType
		{
			get
			{
				return _colorType;
			}
		}

		public IRgbColor RgbColor
		{
			get
			{
				return _rgbColor;
			}
		}
		
		public ISvgIccColor IccColor
		{
			get
			{
				throw new NotImplementedException();
			}
		}
		#endregion

		#region Public Methods
		public void SetRgbColor (string rgbColor)
		{
			SetColor(SvgColorType.RgbColor, rgbColor, String.Empty);
		}
		
		public void SetRgbColorIccColor (string rgbColor, string iccColor )
		{
			SetColor(SvgColorType.RgbColorIccColor, rgbColor, iccColor);
		}
		public void SetColor (SvgColorType colorType, string rgbColor, string iccColor )
		{
			_colorType = colorType;
			if(rgbColor != null && rgbColor.Length > 0)
			{
				try
				{
					_rgbColor = new RgbColor(rgbColor);
				}
				catch(DomException domExc)
				{
					throw new SvgException(SvgExceptionType.SvgInvalidValueErr, "Invalid color value: " + rgbColor, domExc);
				}
			}
			else
			{
				_rgbColor = new RgbColor("black");
			}

			///TODO: deal with ICC colors
		}
		#endregion
	}
}
