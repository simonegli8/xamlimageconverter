using System;
using System.Windows.Media;

namespace SharpVectors.Dom.Css
{
	public class CssPrimitiveRgbValue : CssPrimitiveValue
	{
		public static bool IsColorName(string cssText)
		{
			cssText = cssText.Trim();
			cssText = cssText.Replace("grey", "gray");
			
			try
			{
                Color color = (Color) ColorConverter.ConvertFromString(cssText);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public CssPrimitiveRgbValue(string cssText, bool readOnly) : base(cssText, readOnly)
		{
			_setCssText(cssText);
		}

		protected internal override void _setCssText(string cssText)
		{
			colorValue = new RgbColor(cssText);
			_setType(CssPrimitiveType.RgbColor);
		}

		public override string CssText
		{
			get
			{
				return colorValue.CssText;
			}
			set
			{
				if(ReadOnly)
				{
					throw new DomException(DomExceptionType.InvalidModificationErr, "CssPrimitiveValue is read-only");
				}
				else
				{
					_setCssText(value);
				}
			}
		}
	}
}
