using System;
using System.Xml;

using System.Diagnostics;
using System.Windows.Media;
using System.Windows;



namespace SharpVectors.Dom.Svg
{
    public delegate void InvalidateEventHandler(Object sender, EventArgs e);

    /// <summary>
    /// The SVGCircleElement interface corresponds to the 'rect' element. 
    /// </summary>
    /// <developer>niklas@protocol7.com</developer>
    /// <completed>100</completed>
    public class SvgCircleElement : SvgTransformableElement, ISharpPathGeometry, ISvgCircleElement
    {
        #region Private Fields

        #endregion

        #region Constructors

        internal SvgCircleElement(string prefix, string localname, string ns, SvgDocument doc)
            : base(prefix, localname, ns, doc)
        {
            svgExternalResourcesRequired = new SvgExternalResourcesRequired(this);
            svgTests = new SvgTests(this);
        }

        #endregion

        #region Public Properties
        private ISvgAnimatedLength cx;
        public ISvgAnimatedLength Cx
        {
            get
            {
                if (cx == null)
                {
                    cx = new SvgAnimatedLength("cx", GetAttribute("cx"), "0", this, SvgLengthDirection.Horizontal);
                }
                return cx;
            }
        }

        private ISvgAnimatedLength cy;
        public ISvgAnimatedLength Cy
        {
            get
            {
                if (cy == null)
                {
                    cy = new SvgAnimatedLength("cy", GetAttribute("cy"), "0", this, SvgLengthDirection.Vertical);
                }
                return cy;
            }

        }

        private ISvgAnimatedLength r;
        public ISvgAnimatedLength R
        {
            get
            {
                if (r == null)
                {
                    r = new SvgAnimatedLength("r", GetAttribute("r"), "100", this, SvgLengthDirection.Viewport);
                }
                return r;
            }
        }

        #endregion

        #region Public Methods

        public void Invalidate()
        {

        }

        public Geometry GetGeometry()
        {

            return new EllipseGeometry(new Point(Cx.AnimVal.Value, Cy.AnimVal.Value), R.AnimVal.Value, R.AnimVal.Value);

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

        #region Update handling
        public override void OnAttributeChange(XmlNodeChangedAction action, XmlAttribute attribute)
        {
            base.OnAttributeChange(action, attribute);

            if (attribute.NamespaceURI.Length == 0)
            {
                switch (attribute.LocalName)
                {
                    case "cx":
                        cx = null;
                        break;
                    case "cy":
                        cy = null;
                        break;
                    case "r":
                        r = null;
                        break;
                }
            }
        }
        #endregion
    }
}
