using System;

using System.Xml;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;

namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// Summary description for SvgClipPathElement.
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>100</completed>
	public class SvgClipPathElement : SvgTransformableElement, ISharpPathGeometry, ISvgClipPathElement, ISharpDoNotPaint
	{
		#region Private Fields
        //private PathGeometry _gp = null;

		#endregion
		
		#region	Constructors
		internal SvgClipPathElement(string prefix, string localname, string ns, SvgDocument doc)
			: base(prefix, localname, ns, doc)
		{
			svgExternalResourcesRequired = new SvgExternalResourcesRequired(this);
			svgTests = new SvgTests(this);
		}
		#endregion

		#region Public Properties
		/// <summary>
		/// Default : userSpaceOnUse.
		/// </summary>
		public ISvgAnimatedEnumeration ClipPathUnits
		{
			get
			{
				SvgUnitType clipPath = SvgUnitType.UserSpaceOnUse;
				if(GetAttribute("clipPathUnits") == "objectBoundingBox") 
					clipPath = SvgUnitType.ObjectBoundingBox;
				return new SvgAnimatedEnumeration(clipPath);
			}
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

		#region Public Methods

		public void Invalidate()
		{
			//_gp = null;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <returns>Aggregate graphics path of children.</returns>
		public Geometry GetGeometry()
		{
            //if(_gp == null)
            //{
                PathGeometry _gp = new PathGeometry();

				foreach( XmlNode node in this.ChildNodes )
				{					
					if (node is ISharpPathGeometry && node is SvgStyleableElement)
					{
                        Geometry child = ((ISharpPathGeometry)node).GetGeometry();
                        
                        string clipRule = ((SvgStyleableElement)node).GetPropertyValue("clip-rule");
                        _gp.FillRule = (clipRule == "evenodd") ? FillRule.EvenOdd : FillRule.Nonzero;

                        _gp.AddGeometry(child);

                        //GraphicsPath childPath = ((ISharpUIElement) node).GetGeometry();
						
                        //string clipRule = ((SvgStyleableElement) node).GetPropertyValue("clip-rule");
                        //_gp.FillMode = (clipRule == "evenodd") ? FillMode.Alternate : FillMode.Winding;																															

                        //_gp.AddPath( childPath, true);						
					}
				}
			//}
			return _gp;
		}

		#endregion
	}
}
