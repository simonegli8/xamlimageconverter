using System;

namespace SharpVectors.Dom.Css
{
	public class CssPrimitiveColorValue : CssPrimitiveValue
	{
		public CssPrimitiveColorValue(int color, bool readOnly) : base(color.ToString(CssNumber.Format), readOnly)
		{
			_setFloatValue(color);
			_setType(CssPrimitiveType.Number);
		}

		public CssPrimitiveColorValue(string cssText, bool readOnly) : base(cssText, readOnly)
		{
			_setCssText(cssText);
		}

		protected internal override void _setCssText(string cssText)
		{
			if(cssText.EndsWith("%"))
			{
				cssText = cssText.Remove(cssText.Length-1, 1);
				_setType(CssPrimitiveType.Percentage);
			}
			else
			{
				_setType(CssPrimitiveType.Number);
				
			}
			_setFloatValue(cssText);
		}

		public override double GetFloatValue(CssPrimitiveType unitType)
		{
			double ret = Double.NaN;
			switch(unitType)
			{
				case CssPrimitiveType.Number:
					if(PrimitiveType == CssPrimitiveType.Number) ret = floatValue;
					else if(PrimitiveType == CssPrimitiveType.Percentage)
					{
                        ret = floatValue / 100 * 255D;
					}
					break;
				case CssPrimitiveType.Percentage:
					if(PrimitiveType == CssPrimitiveType.Percentage) ret = floatValue;
					else if(PrimitiveType == CssPrimitiveType.Number)
					{
						ret = floatValue * 255D;
					}
					break;
			}
			if(Double.IsNaN(ret))
			{
				throw new DomException(DomExceptionType.InvalidAccessErr);
			}
			else
			{
				return ret;
			}
		}

		public override string CssText
		{
			get
			{
				double dbl = GetFloatValue(CssPrimitiveType.Number);
				return Convert.ToInt32(dbl).ToString();
			}
		}
	}
}
