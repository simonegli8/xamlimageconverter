using System;
using System.Xml;
using System.Collections;

using SharpVectors.Dom.Css;

namespace SharpVectors.Dom.Svg
{
    /// <summary>    /// A class to encapsulate all SvgTest functionality.  Used by SVG elements as a helper class    /// </summary>
    /// <developer>kevin@kevlindev.com</developer>
	public class SvgTests : ISvgTests
	{
        #region Constructor
		public SvgTests(SvgElement ownerElement)
		{
			this.ownerElement = ownerElement;
			this.ownerElement.AttributeChange += new AttributeChangeHandler(onAttributeChange);
        }
        #endregion

		#region Private fields
		private SvgElement ownerElement;
		#endregion

        #region ISvgTest Interface
		private SvgStringList requiredFeatures;
        public ISvgStringList RequiredFeatures
        {
            get 
			{ 
				if(requiredFeatures == null)
				{
                    requiredFeatures = new SvgStringList(ownerElement.GetAttribute("requiredFeatures"));
				}
				return requiredFeatures; 
			}
        }

        
		private SvgStringList requiredExtensions;
		public ISvgStringList RequiredExtensions
        {
			get 
			{ 
				if(requiredExtensions == null)
				{
					requiredExtensions = new SvgStringList(ownerElement.GetAttribute("requiredExtensions"));
				}
				return requiredExtensions; 
			}
        }

        
		private SvgStringList systemLanguage;
		public ISvgStringList SystemLanguage
        {
			get 
			{ 
				if(systemLanguage == null)
				{
					systemLanguage = new SvgStringList(ownerElement.GetAttribute("systemLanguage"));
				}
				return systemLanguage; 
			}
        }

        public bool HasExtension(string extension)
        {
            bool result = false;

			for ( int i = 0; i < RequiredExtensions.NumberOfItems; i++ )
            {
                if ( RequiredExtensions.GetItem(i) == extension )
                {
                    result = true;
                    break;
                }
            }
            
            return result;
        }
        #endregion

		#region Update handling
		private void onAttributeChange(XmlNodeChangedAction action, XmlAttribute attribute)
		{
			if(attribute.NamespaceURI.Length == 0)
			{
				switch(attribute.LocalName)
				{
					case "requiredFeatures":
						requiredFeatures = null;
						break;
					case "requiredExtensions":
						requiredExtensions = null;
						break;
					case "systemLanguage":
						systemLanguage = null;
						break;
				}
			}
		}
		#endregion
	}
}
