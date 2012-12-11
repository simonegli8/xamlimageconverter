using System;
using System.Xml;

using SharpVectors.Dom.Svg.Rendering;
using System.Windows.Media;

namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// The SvgSwitchElement interface corresponds to the 'switch' 
	/// element. 
	/// </summary>
	public class SvgSwitchElement : SvgTransformableElement , ISvgSwitchElement
	{
		#region Constructors

		internal SvgSwitchElement(string prefix, string localname, string ns, SvgDocument doc) : base(prefix, localname, ns, doc) 
		{
			svgExternalResourcesRequired = new SvgExternalResourcesRequired(this);
			svgTests = new SvgTests(this);
		}

		#endregion

		#region Implementation of ISvgExternalResourcesRequired
		private SvgExternalResourcesRequired svgExternalResourcesRequired;
		public ISvgAnimatedBoolean ExternalResourcesRequired
		{
			get
			{
				return svgExternalResourcesRequired.ExternalResourcesRequired;
			}
		}
		#endregion

		#region Implementation of ISvgTests
		private SvgTests svgTests;
		public ISvgStringList RequiredFeatures
		{
			get { return svgTests.RequiredFeatures; }
		}

		public ISvgStringList RequiredExtensions
		{
			get { return svgTests.RequiredExtensions; }
		}

		public ISvgStringList SystemLanguage
		{
			get { return svgTests.SystemLanguage; }
		}

		public bool HasExtension(string extension)
		{
			return svgTests.HasExtension(extension);
		}
        #endregion

		private bool passesAllTest(ISvgTests element)
		{
			if(element.RequiredFeatures.NumberOfItems>0)
			{
				foreach(string req in (SvgStringList)element.RequiredFeatures)
				{
					if(!SvgDocument.SupportedFeatures.Contains(req)) return false;
				}
			}

			if(element.RequiredExtensions.NumberOfItems>0)
			{
				foreach(string req in (SvgStringList)element.RequiredExtensions)
				{
					if(!SvgDocument.SupportedExtensions.Contains(req)) return false;
				}
			}
			if(element.SystemLanguage.NumberOfItems>0)
			{
				// TODO: or if one of the languages indicated by user preferences exactly equals a prefix of one of the languages given in the value of this parameter such that the first tag character following the prefix is "-".
				bool found = false;
				string currentLang = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

				foreach(string req in (SvgStringList)element.SystemLanguage)
				{
					if(req == currentLang) found = true;
				}
				if(!found) return false;
			}
			return true;
		}

		public override void RenderChildren(ISvgRenderer renderer)
		{
			foreach ( XmlNode node in ChildNodes ) 			{
				SvgElement element = node as SvgElement;
				ISvgTests testsElement = node as ISvgTests;
				if ( element != null && testsElement != null && passesAllTest(testsElement))
				{
                    element.Render(renderer);

					// make sure we only render the first element that passes
					break;
				}
			}
		}
	}
}
