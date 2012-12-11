using System;
using System.Xml;

namespace SharpVectors.Dom.Css
{

	public class CssAbsPrimitiveValue : CssPrimitiveValue
	{
		public CssAbsPrimitiveValue(CssPrimitiveValue cssValue, string propertyName, XmlElement element)
		{
			_cssValue = cssValue;
			_propertyName = propertyName;
			_element = element;
		}

		private CssPrimitiveValue _cssValue;
		private string _propertyName;
		private XmlElement _element;

		public override string CssText
		{
			get
			{
				return _cssValue.CssText;
			}
		}

		public override double GetFloatValue(CssPrimitiveType unitType)
		{
			return _cssValue.GetFloatValue(unitType);
		}

		public override string GetStringValue()
		{
			switch(PrimitiveType)
			{
				case CssPrimitiveType.Attr:
					return _element.GetAttribute(_cssValue.GetStringValue(), String.Empty);
				default:
					return _cssValue.GetStringValue();
			}
		}

		public override IRect GetRectValue()
		{
			return _cssValue.GetRectValue();
		}

		public override IRgbColor GetRgbColorValue()
		{
			return _cssValue.GetRgbColorValue();
		}

		public override CssPrimitiveType PrimitiveType
		{
			get
			{
				return _cssValue.PrimitiveType;
			}
		}
	}
}
