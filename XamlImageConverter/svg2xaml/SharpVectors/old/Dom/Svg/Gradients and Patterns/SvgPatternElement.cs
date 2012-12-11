using System;
using System.Xml;

namespace SharpVectors.Dom.Svg
{

	public class SvgPatternElement : SvgStyleableElement, ISvgPatternElement, ISharpDoNotPaint
	{
		internal SvgPatternElement(string prefix, string localname, string ns, SvgDocument doc)
			: base(prefix, localname, ns, doc)
		{
			svgURIReference = new SvgURIReference(this);
			svgExternalResourcesRequired = new SvgExternalResourcesRequired(this);
			svgFitToViewBox = new SvgFitToViewBox(this);
			svgTests = new SvgTests(this);
		}

		#region Implementation of ISvgPatternElement
		private ISvgAnimatedEnumeration patternUnits;
		public ISvgAnimatedEnumeration PatternUnits
		{
			get
			{
				if(!HasAttribute("patternUnits") && ReferencedElement != null)
				{
					return ReferencedElement.PatternUnits;
				}
				else
				{
					if(patternUnits == null)
					{
						SvgUnitType type;
						switch(GetAttribute("patternUnits"))
						{
							case "userSpaceOnUse":
								type = SvgUnitType.UserSpaceOnUse;
								break;
							default:
								type = SvgUnitType.ObjectBoundingBox;
								break;
						}
						patternUnits = new SvgAnimatedEnumeration(type);
					}
					return patternUnits;
				}
			}
		}

		private ISvgAnimatedEnumeration patternContentUnits;
		public ISvgAnimatedEnumeration PatternContentUnits
		{
			get
			{
				if(!HasAttribute("patternContentUnits") && ReferencedElement != null)
				{
					return ReferencedElement.PatternContentUnits;
				}
				else
				{
					if(patternContentUnits == null)
					{
						SvgUnitType type;
						switch(GetAttribute("patternContentUnits"))
						{
							case "objectBoundingBox":
								type = SvgUnitType.ObjectBoundingBox;
								break;
							default:
								type = SvgUnitType.UserSpaceOnUse;
								break;
						}
						patternContentUnits = new SvgAnimatedEnumeration(type);
					}
					return patternContentUnits;
				}
			}
		}

		private ISvgAnimatedTransformList patternTransform;
		public ISvgAnimatedTransformList PatternTransform
		{
			get
			{
				if(!HasAttribute("patternTransform") && ReferencedElement != null)
				{
					return ReferencedElement.PatternTransform;
				}
				else{
					if(patternTransform == null)
					{
						patternTransform = new SvgAnimatedTransformList(GetAttribute("patternTransform"));
					}
					return patternTransform;
				}
			}
		}

		private ISvgAnimatedLength x;
		public ISvgAnimatedLength X
		{
			get
			{
				if(!HasAttribute("x") && ReferencedElement != null)
				{
					return ReferencedElement.X;
				}
				else{
					if(x == null)
					{
						x = new SvgAnimatedLength("x", GetAttribute("x"), "0", this, SvgLengthDirection.Horizontal);
					}
					return x;
				}
			}
		}

		private ISvgAnimatedLength y;
		public ISvgAnimatedLength Y
		{
			get
			{
				if(!HasAttribute("y") && ReferencedElement != null)
				{
					return ReferencedElement.Y;
				}
				else
				{
					if(y == null)
					{
						y = new SvgAnimatedLength("y", GetAttribute("y"), "0", this, SvgLengthDirection.Vertical);
					}
					return y;
				}
			}
		}

		private ISvgAnimatedLength width;
		public ISvgAnimatedLength Width
		{
			get
			{
				if(!HasAttribute("width") && ReferencedElement != null)
				{
					return ReferencedElement.Width;
				}
				else
				{
					if(width == null)
					{
						width = new SvgAnimatedLength("width", GetAttribute("width"), "0", this, SvgLengthDirection.Horizontal);
					}
					return width;
				}
			}
		}

		private ISvgAnimatedLength height;
		public ISvgAnimatedLength Height
		{
			get
			{
				if(!HasAttribute("height") && ReferencedElement != null)
				{
					return ReferencedElement.Height;
				}
				else
				{
					if(height == null)
					{
						height = new SvgAnimatedLength("height", GetAttribute("height"), "0", this, SvgLengthDirection.Vertical);
					}
					return height;
				}
			}
		}
		#endregion

		#region Implementation of ISvgURIReference
		private SvgURIReference svgURIReference;
		public ISvgAnimatedString Href
		{
			get
			{
				return svgURIReference.Href;
			}
		}

		public SvgPatternElement ReferencedElement
		{
			get
			{
				return svgURIReference.ReferencedNode as SvgPatternElement;
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

		#region Implementation of ISvgFitToViewBox
		private SvgFitToViewBox svgFitToViewBox;
		public ISvgAnimatedRect ViewBox
		{
			get
			{
				return svgFitToViewBox.ViewBox;
			}
		}

		public ISvgAnimatedPreserveAspectRatio PreserveAspectRatio
		{
			get
			{
				return svgFitToViewBox.PreserveAspectRatio;
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

		#region Other public fields
		public XmlNodeList Children
		{
			get
			{
				XmlNodeList children = SelectNodes("svg:*", OwnerDocument.NamespaceManager);
				if(children.Count > 0)
				{
					return children;
				}
				else
				{
					// check any eventually referenced gradient
					if(ReferencedElement == null)
					{
						// return an empty list
						return children;
					}
					else
					{
						return ReferencedElement.Children;
					}
				}
			}
		}
		#endregion
	}
}
