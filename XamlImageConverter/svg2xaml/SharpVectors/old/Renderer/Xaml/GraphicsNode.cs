using System;
using System.Xml;
using System.Collections;
using System.Diagnostics;
using System.Text.RegularExpressions;
using SharpVectors.Dom.Svg;
using SharpVectors.Dom.Svg.Rendering;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows;

namespace SharpVectors.Renderer.Xaml
{
    public class GraphicsNode : RenderingNode
    {
        #region Constructor
        public GraphicsNode(SvgElement element)
            : base(element)
        {
        }
        #endregion

        #region Fields
        //private Color _uniqueColor = Colors.Red;
        //public Color UniqueColor
        //{
        //    get { return _uniqueColor; }
        //}

        ////protected GraphicsContainer graphicsContainer;
        //public Matrix TransformMatrix = Matrix.Identity;
        //private Matrix oldTransformMatrix = Matrix.Identity;
        #endregion

        #region Protected Methods
        //protected void Clip(GraphicsWrapper gr)
        //{

        //    if ( element is SvgStyleableElement )
        //    {
        //        string sClipPath =  ((SvgStyleableElement) element).GetPropertyValue("clip-path");
        //        if (sClipPath != null && sClipPath.Length > 0 && !sClipPath.Equals("none"))
        //        {
        //            String uri = sClipPath.Substring(sClipPath.IndexOf("#"));
        //            uri = uri.Substring(0, uri.Length-1);
        //            Uri absoluteUri = element.ResolveUri(uri);
        //            SvgClipPathElement eClipPath = element.OwnerDocument.GetNodeByUri(absoluteUri) as SvgClipPathElement;

        //            if ( eClipPath != null )
        //            {
        //                PathGeometry gpClip = ((ISharpPathGeometry) eClipPath).GetGeometry();

        //                SvgUnitType pathUnits = (SvgUnitType)eClipPath.ClipPathUnits.AnimVal;

        //                if ( pathUnits == SvgUnitType.ObjectBoundingBox )
        //                {
        //                    PathGeometry gp = ((ISharpPathGeometry)element).GetGeometry();

        //                    gp.Cl
        //                    double fLeft = gp.GetBounds().Left;
        //                    double fTop = gp.GetBounds().Top;
        //                    double boundsWidth = gp.GetBounds().Width;
        //                    double boundsHeight = gp.GetBounds().Height;

        //                    // scale clipping path
        //                    Matrix matrix = new Matrix();
        //                    matrix.Scale(boundsWidth, boundsHeight);
        //                    gpClip.Transform(matrix);
        //                    gr.SetClip(gpClip);									

        //                    // offset clip
        //                    gr.TranslateClip( fLeft, fTop );
        //                }
        //                else
        //                {
        //                    gr.SetClip(gpClip);									
        //                }
        //            }
        //        }
        //    }
        //}

        //protected void SetQuality(GraphicsWrapper gr)
        //{
        //    // TODO: this should read the *-rendering hints
        //    gr.SmoothingMode = SmoothingMode.HighQuality;	
        //}

        //protected void Transform()
        //{
        //    if (element is ISvgTransformable)
        //    {
        //        oldTransformMatrix = TransformMatrix;
        //        TransformMatrix = GetTransformMatrix();
        //    }
        //}

        public static Matrix GetTransformMatrix(SvgElement element)
        {
            ISvgTransformable transElm = element as ISvgTransformable;
            if (transElm == null)
                return Matrix.Identity;

            SvgTransformList svgTList = (SvgTransformList)transElm.Transform.AnimVal;
            //SvgTransform svgTransform = (SvgTransform)svgTList.Consolidate();
            SvgMatrix svgMatrix = ((SvgTransformList)transElm.Transform.AnimVal).TotalMatrix;

            return new Matrix(
                (double)svgMatrix.A,
                (double)svgMatrix.B,
                (double)svgMatrix.C,
                (double)svgMatrix.D,
                (double)svgMatrix.E,
                (double)svgMatrix.F);
        }

        //protected void fitToViewbox(GraphicsWrapper graphics, Rect elmRect)
        //{
        //    if ( element is ISvgFitToViewBox )
        //    {
        //        ISvgFitToViewBox fitToVBElm = (ISvgFitToViewBox) element;
        //        SvgPreserveAspectRatio spar = (SvgPreserveAspectRatio)fitToVBElm.PreserveAspectRatio.AnimVal;

        //        double[] translateAndScale = spar.FitToViewBox(
        //            (SvgRect)fitToVBElm.ViewBox.AnimVal,
        //            new SvgRect(elmRect.X, elmRect.Y, elmRect.Width, elmRect.Height)
        //            );
        //        graphics.TranslateTransform(translateAndScale[0], translateAndScale[1]);
        //        graphics.ScaleTransform(translateAndScale[2], translateAndScale[3]);
        //    }
        //}
        #endregion

        #region Public Methods
        //public override void BeforeRender(ISvgRenderer renderer)
        //{
        //    _uniqueColor = ((XamlRenderer)renderer)._getNextColor(this);

        //    base.BeforeRender(renderer);
        //    //GraphicsWrapper graphics = ((XamlRenderer) renderer).GraphicsWrapper;

        //    //graphicsContainer = graphics.BeginContainer();
        //    //SetQuality(graphics);
        //    Transform();
        //    //Clip(graphics);
        //}

        //public override void AfterRender(ISvgRenderer renderer)
        //{
        //    RemoveTransform();
        //    base.AfterRender(renderer);
        //    //GraphicsWrapper graphics = ((XamlRenderer)renderer).GraphicsWrapper;

        //    //graphics.EndContainer(graphicsContainer);
        //}

        //public void RemoveTransform()
        //{
        //    TransformMatrix = oldTransformMatrix;
        //}
        #endregion

        public override bool CanRenderChildren(ISvgRenderer renderer)
        {
            return Element.GetAttribute("display") != "none";
        }

    }
}
