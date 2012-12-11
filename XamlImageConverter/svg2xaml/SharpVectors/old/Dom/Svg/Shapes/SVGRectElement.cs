using System;
using System.Xml;
using System.Diagnostics;
using System.Windows.Media;


using SharpVectors.Dom.Events;
using System.Windows;
using System.Windows.Controls;


namespace SharpVectors.Dom.Svg
{
    /// <summary>
    /// The SVGRectElement interface corresponds to the 'rect' element. 
    /// </summary>
    /// <developer>niklas@protocol7.com</developer>
    /// <completed>100</completed>
    public class SvgRectElement : SvgTransformableElement, ISharpPathGeometry, ISvgRectElement, IEventTarget, ISharpShape
    {
        #region Private Fields

        #endregion

        #region Constructors

        internal SvgRectElement(string prefix, string localname, string ns, SvgDocument doc)
            : base(prefix, localname, ns, doc)
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

        #region Implementation of ISvgRectElement

        private ISvgAnimatedLength width;
        public ISvgAnimatedLength Width
        {
            get
            {
                if (width == null)
                {
                    width = new SvgAnimatedLength("width", this.GetAttribute("width"), "100", this, SvgLengthDirection.Horizontal);
                }
                return width;
            }
        }

        private ISvgAnimatedLength height;
        public ISvgAnimatedLength Height
        {
            get
            {
                if (height == null)
                {
                    height = new SvgAnimatedLength("height", this.GetAttribute("height"), "100", this, SvgLengthDirection.Vertical);
                }
                return height;
            }

        }

        private ISvgAnimatedLength x;
        public ISvgAnimatedLength X
        {
            get
            {
                if (x == null)
                {
                    x = new SvgAnimatedLength("x", this.GetAttribute("x"), "0", this, SvgLengthDirection.Horizontal);
                }
                return x;
            }

        }

        private ISvgAnimatedLength y;
        public ISvgAnimatedLength Y
        {
            get
            {
                if (y == null)
                {
                    y = new SvgAnimatedLength("y", this.GetAttribute("y"), "0", this, SvgLengthDirection.Vertical);
                }
                return y;
            }

        }

        private ISvgAnimatedLength rx;
        public ISvgAnimatedLength Rx
        {
            get
            {
                if (rx == null)
                {
                    rx = new SvgAnimatedLength("rx", this.GetAttribute("rx"), "0", this, SvgLengthDirection.Horizontal);
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
                    ry = new SvgAnimatedLength("ry", this.GetAttribute("ry"), "0", this, SvgLengthDirection.Vertical);
                }
                return ry;
            }
        }

        #endregion

        #region Implementation of ISharpGDIPath

        public void Invalidate()
        {
        }


        public Geometry GetGeometry()
        {

            Rect rect = new Rect((double)X.AnimVal.Value, (double)Y.AnimVal.Value, (double)Width.AnimVal.Value, (double)Height.AnimVal.Value);

            double rx = (double)Rx.AnimVal.Value;
            double ry = (double)Ry.AnimVal.Value;

            return new RectangleGeometry(rect, rx, ry);

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

        #region Events
        public bool DispatchEvent(IEvent e)
        {
            return ((Event)e).Propagate(this, true);
        }

        public void FireEvent(IEvent e)
        {
            DomEvent eventToFire = null;

            switch (e.Type)
            {
                case "mousemove":
                    eventToFire = OnMouseMove;
                    break;
                case "mouseup":
                    eventToFire = OnMouseUp;
                    break;
                case "mousedown":
                    eventToFire = OnMouseDown;
                    break;
                case "mouseover":
                    eventToFire = OnMouseOver;
                    break;
                case "mouseout":
                    eventToFire = OnMouseOut;
                    break;
                case "click":
                    eventToFire = OnClick;
                    break;
            }

            if (eventToFire != null) eventToFire(e);
        }

        public event DomEvent OnMouseMove;
        public event DomEvent OnMouseDown;
        public event DomEvent OnMouseUp;
        public event DomEvent OnMouseOut;
        public event DomEvent OnMouseOver;
        public event DomEvent OnClick;
        #endregion

        #region Update handling
        public override void OnAttributeChange(XmlNodeChangedAction action, XmlAttribute attribute)
        {
            base.OnAttributeChange(action, attribute);

            if (attribute.NamespaceURI.Length == 0)
            {
                switch (attribute.LocalName)
                {
                    case "x":
                        x = null;
                        break;
                    case "y":
                        y = null;
                        break;
                    case "width":
                        width = null;
                        break;
                    case "height":
                        height = null;
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
        public System.Windows.Shapes.Shape GetShape()
        {
            System.Windows.Shapes.Rectangle shape = new System.Windows.Shapes.Rectangle();
            shape.Width = (double)Width.AnimVal.Value;
            shape.Height = (double)Height.AnimVal.Value;
            Canvas.SetLeft(shape, (double)X.AnimVal.Value);
            Canvas.SetTop(shape, (double)Y.AnimVal.Value);
            shape.RadiusX = (double)Rx.AnimVal.Value;
            shape.RadiusY = (double)Ry.AnimVal.Value;

            return shape;
        }

        #endregion
    }
}
