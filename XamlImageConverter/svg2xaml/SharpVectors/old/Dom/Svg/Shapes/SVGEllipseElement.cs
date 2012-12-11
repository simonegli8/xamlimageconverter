using System;
using System.Xml;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;




namespace SharpVectors.Dom.Svg
{
    /// <summary>
    /// The SvgEllipseElement class corresponds to the 'ellipse' element. 
    /// </summary>
    /// <developer>niklas@protocol7.com</developer>
    /// <completed>100</completed>
    public class SvgEllipseElement : SvgTransformableElement, ISharpPathGeometry, ISvgEllipseElement, ISharpShape
    {

        #region Private Fields

        //private EllipseGeometry gp = null;

        #endregion

        #region Constructors

        internal SvgEllipseElement(string prefix, string localname, string ns, SvgDocument doc)
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
                    cx = new SvgAnimatedLength("cx", this.GetAttribute("cx"), "0", this, SvgLengthDirection.Horizontal);
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
                    cy = new SvgAnimatedLength("cy", this.GetAttribute("cy"), "0", this, SvgLengthDirection.Vertical);
                }
                return cy;
            }

        }

        private ISvgAnimatedLength rx;
        public ISvgAnimatedLength Rx
        {
            get
            {
                if (rx == null)
                {
                    return new SvgAnimatedLength("rx", this.GetAttribute("rx"), "100", this, SvgLengthDirection.Horizontal);
                }
                return rx;
            }

        }

        private ISvgAnimatedLength ry;
        public ISvgAnimatedLength Ry
        {
            get
            {
                if (ry == null)
                {
                    ry = new SvgAnimatedLength("ry", this.GetAttribute("ry"), "100", this, SvgLengthDirection.Vertical);
                }
                return ry;
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
            //gp = null;
        }

        public Geometry GetGeometry()
        {

            Geometry gp = new EllipseGeometry(new Point(Cx.AnimVal.Value, Cy.AnimVal.Value), Rx.AnimVal.Value, Ry.AnimVal.Value);


            return gp;
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
                    case "rx":
                        rx = null;
                        break;
                    case "ry":
                        ry = null;
                        break;
                }
            }
        }
        #endregion

        #region ISharpShape Members
        //private Ellipse shape;
        public Shape GetShape()
        {
            //if (shape == null)
            //{
            Ellipse shape = new Ellipse();
            shape.Width = 2 * Rx.AnimVal.Value;
            shape.Height = 2 * Ry.AnimVal.Value;
            Canvas.SetLeft(shape, Cx.AnimVal.Value - Rx.AnimVal.Value);
            Canvas.SetTop(shape, Cy.AnimVal.Value - Ry.AnimVal.Value);
            //}

            return shape;
        }

        #endregion
    }
}
