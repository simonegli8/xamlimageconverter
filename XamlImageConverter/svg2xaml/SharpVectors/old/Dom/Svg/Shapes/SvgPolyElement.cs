using System;
using System.Windows.Media;
using System.Xml;
using System.Windows;
using System.Windows.Shapes;


namespace SharpVectors.Dom.Svg
{
    public abstract class SvgPolyElement : SvgTransformableElement, ISharpPathGeometry, ISharpMarkerHost
    {
        #region Contructors
        internal SvgPolyElement(string prefix, string localname, string ns, SvgDocument doc)
            : base(prefix, localname, ns, doc)
        {
            svgExternalResourcesRequired = new SvgExternalResourcesRequired(this);
            svgTests = new SvgTests(this);
        }
        #endregion

        #region Protected Fields
        #endregion

        #region Public Properties
        public ISvgPointList AnimatedPoints
        {
            get
            {
                return Points;
            }
        }

        private ISvgPointList points;
        public ISvgPointList Points
        {
            get
            {
                if (points == null)
                {
                    points = new SvgPointList(GetAttribute("points"));
                }
                return points;
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

        #region Implementation of ISharpGDIPath
        public virtual void Invalidate()
        {
        }

        public Geometry GetGeometry()
        {
            PathGeometry gp = new PathGeometry();
            
            ISvgPointList list = AnimatedPoints;
            int nElems = list.NumberOfItems;
            if (nElems > 0)
            {
                Point start = new Point(list.GetItem(0).X, list.GetItem(0).Y);
                Point[] points = new Point[nElems - 1];

                for (int i = 1; i < nElems; i++)
                {
                    points[i - 1] = new Point((double)list.GetItem(i).X, (double)list.GetItem(i).Y);

                }

                PathFigure fig = new PathFigure();
                fig.StartPoint = start;
                gp.Figures.Add(fig);

                if (this is SvgPolygonElement)
                {
                    fig.IsClosed = true;
                }
                fig.Segments.Add(new PolyLineSegment(points, true));
            }

            string fillRule = GetPropertyValue("fill-rule");
            if (fillRule == "evenodd")
                Rendering.RenderingNode.AssignValue(gp, PathGeometry.FillRuleProperty, FillRule.EvenOdd);
            else
                Rendering.RenderingNode.AssignValue(gp, PathGeometry.FillRuleProperty, FillRule.Nonzero);


            return gp;
        }
        #endregion

        #region Implementation of ISharpMarkerHost
        public virtual Point[] MarkerPositions
        {
            get
            {
                // moved this code from SvgPointList.  This should eventually migrate into
                // the GDI+ renderer
                Point[] points = new Point[Points.NumberOfItems];

                for (int i = 0; i < Points.NumberOfItems; i++)
                {
                    SvgPoint point = (SvgPoint)Points.GetItem(i);
                    points[i] = new Point((double)point.X, (double)point.Y);
                }

                return points;
            }
        }

        public double GetStartAngle(int index)
        {
            index--;

            Point[] positions = MarkerPositions;

            if (index > positions.Length - 1)
            {
                throw new Exception("GetStartAngle: index to large");
            }

            Point p1 = positions[index];
            Point p2 = positions[index + 1];

            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;

            double a = (double)(Math.Atan2(dy, dx) * 180 / Math.PI);
            a -= 90;
            a %= 360;
            return a;
        }

        public double GetEndAngle(int index)
        {
            double a = GetStartAngle(index);
            a += 180;
            a %= 360;
            return a;
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
                    case "points":
                        points = null;
                        break;
                }
            }
        }
        #endregion
    }
}
