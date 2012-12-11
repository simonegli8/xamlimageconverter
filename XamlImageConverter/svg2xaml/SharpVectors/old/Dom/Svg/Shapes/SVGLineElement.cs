using System;
using System.Xml;
using System.Windows.Media;
using System.Windows;

namespace SharpVectors.Dom.Svg
{
    /// <summary>
    /// The SVGLineElement interface corresponds to the 'line' element.  
    /// </summary>
    /// <developer>niklas@protocol7.com</developer>
    /// <completed>100</completed>
    public class SvgLineElement : SvgTransformableElement, ISharpPathGeometry, ISharpShape, ISvgLineElement, ISharpMarkerHost
    {
        #region Private Fields


        #endregion

        #region Constructors

        internal SvgLineElement(string prefix, string localname, string ns, SvgDocument doc)
            : base(prefix, localname, ns, doc)
        {
            svgExternalResourcesRequired = new SvgExternalResourcesRequired(this);
            svgTests = new SvgTests(this);
        }

        #endregion

        #region Public Properties
        private ISvgAnimatedLength x1;
        public ISvgAnimatedLength X1
        {
            get
            {
                if (x1 == null)
                {
                    x1 = new SvgAnimatedLength("x1", this.GetAttribute("x1"), "0", this, SvgLengthDirection.Horizontal);
                }
                return x1;
            }
        }

        private ISvgAnimatedLength y1;
        public ISvgAnimatedLength Y1
        {
            get
            {
                if (y1 == null)
                {
                    y1 = new SvgAnimatedLength("y1", this.GetAttribute("y1"), "0", this, SvgLengthDirection.Vertical);
                }
                return y1;
            }

        }

        private ISvgAnimatedLength x2;
        public ISvgAnimatedLength X2
        {
            get
            {
                if (x2 == null)
                {
                    x2 = new SvgAnimatedLength("x2", this.GetAttribute("x2"), "0", this, SvgLengthDirection.Horizontal);
                }
                return x2;
            }
        }

        private ISvgAnimatedLength y2;
        public ISvgAnimatedLength Y2
        {
            get
            {
                if (y2 == null)
                {
                    y2 = new SvgAnimatedLength("y2", this.GetAttribute("y2"), "0", this, SvgLengthDirection.Vertical);
                }
                return y2;
            }
        }

        #endregion

        #region Public Methods

        public void Invalidate()
        {
        }

        public Geometry GetGeometry()
        {
            return new LineGeometry(new Point(X1.AnimVal.Value, Y1.AnimVal.Value), new Point(X2.AnimVal.Value, Y2.AnimVal.Value));
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

        #region Implementation of ISharpMarkerHost
        public Point[] MarkerPositions
        {
            get
            {
                return new Point[] { new Point((double)X1.AnimVal.Value, (double)Y1.AnimVal.Value), new Point((double)X2.AnimVal.Value, (double)Y2.AnimVal.Value) };
            }
        }

        public double GetStartAngle(int index)
        {
            return 0;
        }

        public double GetEndAngle(int index)
        {
            return 0;
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
                    case "x1":
                        x1 = null;
                        break;
                    case "y1":
                        y1 = null;
                        break;
                    case "x2":
                        x2 = null;
                        break;
                    case "y2":
                        y2 = null;
                        break;
                }
            }
        }
        #endregion

        #region ISharpShape Members
        public System.Windows.Shapes.Shape GetShape()
        {

            System.Windows.Shapes.Line lineShape = new System.Windows.Shapes.Line();
            lineShape.X1 = X1.AnimVal.Value;
            lineShape.Y1 = Y1.AnimVal.Value;
            lineShape.X2 = X2.AnimVal.Value;
            lineShape.Y2 = Y2.AnimVal.Value;


            return lineShape;
        }

        #endregion
    }
}
