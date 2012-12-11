using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Diagnostics;
using SharpVectors.Dom.Css;



namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// SvgStyleableElement is an extension to the Svg DOM to create a class for all elements that are styleable.
	/// </summary>
	public class SvgStyleableElement : SvgElement
	{
		#region Private static fields
		private static Regex isImportant = new Regex(@"!\s*important$");
		#endregion

		#region Private Fields
		private Hashtable styleProperties = new Hashtable();
		#endregion

		#region Constructors

		internal SvgStyleableElement(string prefix, string localname, string ns, SvgDocument doc) : base(prefix, localname, ns, doc) 
		{
		}

		#endregion

		#region Public Properties

		public ISvgAnimatedString ClassName
		{
			get
			{
				return new SvgAnimatedString(GetAttribute("class", String.Empty));
			}
		}
		#endregion

		#region Public Methods
		public ICssValue GetPresentationAttribute(string name)
		{
			if(HasAttribute(name, String.Empty) && GetAttribute(name, String.Empty).Length > 0)
			{
				string attValue = GetAttribute(name, String.Empty);
				if(isImportant.IsMatch(attValue))
				{
					return null;
				}
				else
				{
					return CssValue.GetCssValue(attValue, false);
				}
			}
			else
			{
				return null;
			}
		}

		public string GetPropertyValue(string name)
		{
			return GetComputedStyle(String.Empty).GetPropertyValue(name);
		}

		public string GetPropertyValue(string name1, string name2)
		{
			string cssString = GetComputedStyle(String.Empty).GetPropertyValue(name1);
			if(cssString == null)
			{
				cssString = GetComputedStyle(String.Empty).GetPropertyValue(name2);
			}

			return cssString;
		}

		public override ICssStyleDeclaration GetComputedStyle(string pseudoElt)
		{
			if(_cachedCSD == null)
			{
				CssCollectedStyleDeclaration csd = (CssCollectedStyleDeclaration)base.GetComputedStyle(pseudoElt);
				
				IEnumerator cssPropNames = OwnerDocument.CssPropertyProfile.GetAllPropertyNames().GetEnumerator();
				while(cssPropNames.MoveNext())
				{
					string cssPropName = (string)cssPropNames.Current;
					CssValue cssValue = (CssValue)GetPresentationAttribute(cssPropName);
					if(cssValue != null)
					{
						csd.CollectProperty(cssPropName, 0, cssValue, CssStyleSheetType.NonCssPresentationalHints, String.Empty);
					}
				}

				_cachedCSD = csd;
			}
			return _cachedCSD;
		}		
		
		#endregion
	}
}
