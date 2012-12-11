using System;

namespace SharpVectors.Dom.Svg
{
	public class SvgMaskElement : SvgStyleableElement, ISvgMaskElement, ISharpDoNotPaint
	{
		#region Private Fields

		#endregion
		
		#region Constructors

		internal SvgMaskElement(string prefix, string localname, string ns, SvgDocument doc)
			: base(prefix, localname, ns, doc)
		{
			svgExternalResourcesRequired = new SvgExternalResourcesRequired(this);
			svgTests = new SvgTests(this);
		}

		#endregion

		#region Public Properties

		public ISvgAnimatedEnumeration MaskUnits 
		{
			get
			{ 
				SvgUnitType mask = SvgUnitType.ObjectBoundingBox;
				if(GetAttribute("maskUnits") == "userSpaceOnUse") 
					mask = SvgUnitType.UserSpaceOnUse;
				return new SvgAnimatedEnumeration(mask);
			}
		}

		public ISvgAnimatedEnumeration MaskContentUnits 
		{
			get
			{
				SvgUnitType maskContent = SvgUnitType.UserSpaceOnUse;
				if(GetAttribute("maskContentUnits") == "objectBoundingBox") 
					maskContent = SvgUnitType.ObjectBoundingBox;
				return new SvgAnimatedEnumeration(maskContent);
			}
		}

		public ISvgAnimatedLength X 
		{
			get
			{
				return new SvgAnimatedLength("x", this.GetAttribute("x"), "-10%", this, SvgLengthDirection.Horizontal);
			}
		}

		public ISvgAnimatedLength Y 
		{
			get
			{
				return new SvgAnimatedLength("y", this.GetAttribute("y"), "-10%", this, SvgLengthDirection.Vertical);
			}
		}

		public ISvgAnimatedLength Width 
		{
			get
			{
				return new SvgAnimatedLength("width", this.GetAttribute("width"), "120%", this, SvgLengthDirection.Viewport);
			}
		}

		public ISvgAnimatedLength Height 
		{
			get
			{
				return new SvgAnimatedLength("height", this.GetAttribute("height"), "120%", this, SvgLengthDirection.Viewport);
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
	}
}
