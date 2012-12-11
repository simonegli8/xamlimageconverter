using System;
using System.Xml;

namespace SharpVectors.Dom.Svg
{
	public class SvgUseElement : SvgTransformableElement, ISvgUseElement
	{
		#region Constructors
		internal SvgUseElement(string prefix, string localname, string ns, SvgDocument doc) : base(prefix, localname, ns, doc) 
		{
			svgURIReference = new SvgURIReference(this);
			svgExternalResourcesRequired = new SvgExternalResourcesRequired(this);
			svgTests = new SvgTests(this);
		}
		#endregion

        #region Public Methods
		public SvgGElement GetReplacedElement()
		{
            SvgGElement gElm = (SvgGElement)OwnerDocument.CreateElement(String.Empty, "g", SvgDocument.SvgNamespace);
			foreach ( XmlNode att in Attributes )
			{
				if ( att.LocalName != "x" && 
					 att.LocalName != "y" &&
					 att.LocalName != "width" &&
					 att.LocalName != "height" &&
					!(att.LocalName == "href" && att.NamespaceURI == SvgDocument.XLinkNamespace))
				{
					gElm.SetAttribute(att.LocalName, att.Value);
				}
			}

			gElm.SetAttribute("transform", 
				GetAttribute("transform") + " translate(" + X.AnimVal.Value + "," + Y.AnimVal.Value + ")");

			XmlElement refElm = ReferencedElement;

			if ( refElm is SvgSymbolElement )
			{
				SvgSvgElement svgElm = (SvgSvgElement)OwnerDocument.CreateElement(String.Empty, "svg", SvgDocument.SvgNamespace);
				svgElm.SetAttribute("width", (HasAttribute("width")) ? GetAttribute("width") : "100%");
				svgElm.SetAttribute("height", (HasAttribute("height")) ? GetAttribute("height") : "100%");

				foreach (XmlNode att in refElm.Attributes )
				{
					if ( att.LocalName != "width" && att.LocalName != "height")
					{
						svgElm.SetAttribute(att.LocalName, att.Value);
					}
				}

				foreach(XmlNode child in refElm.ChildNodes)
				{
					svgElm.AppendChild(OwnerDocument.ImportNode(child.Clone(), true));
					//svgElm.AppendChild(child.Clone());
				}
         		gElm.AppendChild(svgElm);
			}
			else if ( refElm != null )
			{
				gElm.AppendChild(OwnerDocument.ImportNode(refElm.Clone(), true));
			}
        
			return gElm;
		}
        #endregion

		#region Implementation of ISvgUseElement 
		public ISvgAnimatedLength X
		{
			get
			{
				return (ISvgAnimatedLength)new SvgAnimatedLength("x", GetAttribute("x"), "0", this, SvgLengthDirection.Horizontal);
			}
		}
		public ISvgAnimatedLength Y
		{
			get
			{
				return (ISvgAnimatedLength)new SvgAnimatedLength("y", GetAttribute("y"), "0", this, SvgLengthDirection.Vertical);
			}
		}
		public ISvgAnimatedLength Width
		{
			get
			{
				return (ISvgAnimatedLength)new SvgAnimatedLength("width", GetAttribute("width"), this, SvgLengthDirection.Horizontal);
			}
		}
		public ISvgAnimatedLength Height
		{
			get
			{
				return (ISvgAnimatedLength)new SvgAnimatedLength("height", GetAttribute("height"), this, SvgLengthDirection.Vertical);
			}
		}
		public ISvgElementInstance InstanceRoot
		{
			get
			{
				return (ISvgElementInstance)new SvgElementInstance(ReferencedElement, this, null);
			}
		}
		public ISvgElementInstance AnimatedInstanceRoot
		{
			get
			{
				throw new NotImplementedException("SvgUseElement.animatedInstanceRoot");
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

		public XmlElement ReferencedElement
		{
			get
			{
				return svgURIReference.ReferencedNode as XmlElement;
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
