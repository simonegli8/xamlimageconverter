using System;
using System.Xml;
using System.Windows.Media;

using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;


namespace SharpVectors.Dom.Svg
{
    public class SvgPathElement : SvgTransformableElement, ISvgPathElement, ISharpPathGeometry, ISharpMarkerHost
    {
        #region Constructors
        internal SvgPathElement(string prefix, string localname, string ns, SvgDocument doc)
            : base(prefix, localname, ns, doc)
        {
            svgTests = new SvgTests(this);
        }

        #endregion

        #region Implementation of ISharpMarkerHost
        public Point[] MarkerPositions
        {
            get
            {
                return ((SvgPathSegList)PathSegList).Points;
            }
        }

        public double GetStartAngle(int index)
        {
            return ((SvgPathSegList)PathSegList).GetStartAngle(index);
        }

        public double GetEndAngle(int index)
        {
            return ((SvgPathSegList)PathSegList).GetEndAngle(index);
        }
        #endregion

        #region Implementation of ISharpGDIPath
        public void Invalidate()
        {
        }

        public Geometry GetGeometry()
        {

            PathGeometry gp = new PathGeometry();
            //PathFigure fig = null;
            //Point initPoint = new Point(0, 0);
            //Point lastPoint = new Point(0,0);

            SvgPathSegList segments = (SvgPathSegList)PathSegList;
            //SvgPathSeg segment;

            string fillRule = GetPropertyValue("fill-rule");
            if (fillRule == "evenodd")
                Rendering.RenderingNode.AssignValue(gp, PathGeometry.FillRuleProperty, FillRule.EvenOdd);
            else
                Rendering.RenderingNode.AssignValue(gp, PathGeometry.FillRuleProperty, FillRule.Nonzero);

            try
            {
                gp.Figures = PathFigureCollection.Parse(segments.SvgPathText);
            }
            catch
            {
            }

            return gp;
        }

        #endregion

        #region Implementation of ISvgPathElement
        public ISvgAnimatedBoolean ExternalResourcesRequired
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        private ISvgPathSegList pathSegList;
        public ISvgPathSegList PathSegList
        {
            get
            {
                if (pathSegList == null)
                {
                    pathSegList = new SvgPathSegList(this.GetAttribute("d"), false);
                }
                return pathSegList;
            }
        }

        public ISvgPathSegList NormalizedPathSegList
        {
            get { throw new NotImplementedException(); }
        }

        public ISvgPathSegList AnimatedPathSegList
        {
            get { return PathSegList; }
        }

        public ISvgPathSegList AnimatedNormalizedPathSegList
        {
            get { return NormalizedPathSegList; }
        }

        private ISvgAnimatedNumber _pathLength;
        public ISvgAnimatedNumber PathLength
        {
            get
            {
                if (_pathLength == null)
                {
                    _pathLength = new SvgAnimatedNumber(GetAttribute("pathLength"));
                }
                return _pathLength;
            }
        }

        public double GetTotalLength()
        {
            return ((SvgPathSegList)PathSegList).GetTotalLength();
        }

        public ISvgPoint GetPointAtLength(double distance)
        {
            throw new NotImplementedException();
        }


        public int GetPathSegAtLength(double distance)
        {
            return ((SvgPathSegList)PathSegList).GetPathSegAtLength(distance);
        }

        #region Create methods
        public ISvgPathSegClosePath CreateSvgPathSegClosePath()
        {
            return new SvgPathSegClosePath();
        }

        public ISvgPathSegMovetoAbs CreateSvgPathSegMovetoAbs(double x, double y)
        {
            return new SvgPathSegMovetoAbs(x, y);
        }

        public ISvgPathSegMovetoRel CreateSvgPathSegMovetoRel(double x, double y)
        {
            return new SvgPathSegMovetoRel(x, y);
        }

        public ISvgPathSegLinetoAbs CreateSvgPathSegLinetoAbs(double x, double y)
        {
            return new SvgPathSegLinetoAbs(x, y);
        }

        public ISvgPathSegLinetoRel CreateSvgPathSegLinetoRel(double x, double y)
        {
            return new SvgPathSegLinetoRel(x, y);
        }

        public ISvgPathSegCurvetoCubicAbs CreateSvgPathSegCurvetoCubicAbs(double x,
            double y,
            double x1,
            double y1,
            double x2,
            double y2)
        {
            return new SvgPathSegCurvetoCubicAbs(x, y, x1, y1, x2, y2);
        }

        public ISvgPathSegCurvetoCubicRel CreateSvgPathSegCurvetoCubicRel(double x,
            double y,
            double x1,
            double y1,
            double x2,
            double y2)
        {
            return new SvgPathSegCurvetoCubicRel(x, y, x1, y1, x2, y2);
        }


        public ISvgPathSegCurvetoQuadraticAbs CreateSvgPathSegCurvetoQuadraticAbs(double x,
            double y,
            double x1,
            double y1)
        {
            return new SvgPathSegCurvetoQuadraticAbs(x, y, x1, y1);
        }


        public ISvgPathSegCurvetoQuadraticRel CreateSvgPathSegCurvetoQuadraticRel(double x,
            double y,
            double x1,
            double y1)
        {
            return new SvgPathSegCurvetoQuadraticRel(x, y, x1, y1);
        }


        public ISvgPathSegArcAbs CreateSvgPathSegArcAbs(double x,
            double y,
            double r1,
            double r2,
            double angle,
            bool largeArcFlag,
            bool sweepFlag)
        {
            return new SvgPathSegArcAbs(x, y, r1, r2, angle, largeArcFlag, sweepFlag);
        }


        public ISvgPathSegArcRel CreateSvgPathSegArcRel(double x,
            double y,
            double r1,
            double r2,
            double angle,
            bool largeArcFlag,
            bool sweepFlag)
        {
            return new SvgPathSegArcRel(x, y, r1, r2, angle, largeArcFlag, sweepFlag);
        }


        public ISvgPathSegLinetoHorizontalAbs CreateSvgPathSegLinetoHorizontalAbs(double x)
        {
            return new SvgPathSegLinetoHorizontalAbs(x);
        }

        public ISvgPathSegLinetoHorizontalRel CreateSvgPathSegLinetoHorizontalRel(double x)
        {
            return new SvgPathSegLinetoHorizontalRel(x);
        }

        public ISvgPathSegLinetoVerticalAbs CreateSvgPathSegLinetoVerticalAbs(double y)
        {
            return new SvgPathSegLinetoVerticalAbs(y);
        }

        public ISvgPathSegLinetoVerticalRel CreateSvgPathSegLinetoVerticalRel(double y)
        {
            return new SvgPathSegLinetoVerticalRel(y);
        }

        public ISvgPathSegCurvetoCubicSmoothAbs CreateSvgPathSegCurvetoCubicSmoothAbs(double x,
            double y,
            double x2,
            double y2)
        {
            return new SvgPathSegCurvetoCubicSmoothAbs(x, y, x2, y2);
        }

        public ISvgPathSegCurvetoCubicSmoothRel CreateSvgPathSegCurvetoCubicSmoothRel(double x,
            double y,
            double x2,
            double y2)
        {
            return new SvgPathSegCurvetoCubicSmoothRel(x, y, x2, y2);
        }

        public ISvgPathSegCurvetoQuadraticSmoothAbs CreateSvgPathSegCurvetoQuadraticSmoothAbs(double x,
            double y)
        {
            return new SvgPathSegCurvetoQuadraticSmoothAbs(x, y);
        }

        public ISvgPathSegCurvetoQuadraticSmoothRel CreateSvgPathSegCurvetoQuadraticSmoothRel(double x,
            double y)
        {
            return new SvgPathSegCurvetoQuadraticSmoothRel(x, y);
        }
        #endregion
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
                    case "d":
                        pathSegList = null;
                        break;
                    case "pathLength":
                        _pathLength = null;
                        break;
                }
            }
        }
        #endregion
    }
}
