using System;
using System.Diagnostics;
using SharpVectors.Net;
using SharpVectors.Dom.Svg;
using SharpVectors.Dom.Svg.Rendering;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SharpVectors.Renderer.Xaml
{
	/// <summary>
	/// Summary description for SvgImageGraphicsNode.
	/// </summary>
	public class SvgImageElementGraphicsNode : GraphicsNode
	{
        #region Constructor
		public SvgImageElementGraphicsNode(SvgElement element) : base(element)
		{
		}
        #endregion

		//private SvgWindow window;
		//private SvgDocument document;
		private XamlRenderer gdiRenderer;

		private SvgWindow getSvgWindow()
		{
			SvgWindow parentWindow = (SvgWindow)Element.OwnerDocument.Window;
			SvgImageElement iElm = Element as SvgImageElement;

			SvgWindow wnd = new SvgWindow((double)iElm.Width.AnimVal.Value, (double)iElm.Height.AnimVal.Value, parentWindow);
			gdiRenderer = new XamlRenderer();
			gdiRenderer.Window = wnd;
			wnd.Renderer = gdiRenderer;

			SvgDocument doc = new SvgDocument(wnd);
			wnd.Document = doc;

			Uri absoluteUri = Element.ResolveUri(iElm.Href.AnimVal);

			iElm.ReferencedResource.Stream.Position = 0;
			doc.Load(absoluteUri.AbsoluteUri, iElm.ReferencedResource.Stream);

			return wnd;
		}

        #region Public Methods
		public override void Render(ISvgRenderer renderer)
		{
            if (this.Element.GetAttribute("display") == "none")
                return;

            XamlRenderer xamlRenderer = ((XamlRenderer)renderer);
            SvgImageElement iElement = (SvgImageElement)element;
            HttpResource resource = iElement.ReferencedResource;

            if (resource != null)
            {                
                //ImageAttributes imageAttributes = new ImageAttributes();

                string sOpacity = iElement.GetPropertyValue("opacity");
                double opacity = SvgNumber.ParseToFloat(sOpacity);
                //if (sOpacity.Length > 0)
                //{
                //    double opacity = SvgNumber.ParseToFloat(sOpacity);
                //    ColorMatrix myColorMatrix = new ColorMatrix();
                //    myColorMatrix.Matrix00 = 1.00f; // Red
                //    myColorMatrix.Matrix11 = 1.00f; // Green
                //    myColorMatrix.Matrix22 = 1.00f; // Blue
                //    myColorMatrix.Matrix33 = (double)opacity; // alpha
                //    myColorMatrix.Matrix44 = 1.00f; // w

                //    imageAttributes.SetColorMatrix(myColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                //}

                double width = (double)iElement.Width.AnimVal.Value;
                double height = (double)iElement.Height.AnimVal.Value;

                //Rect destRect = new Rect();
                //destRect.X = iElement.X.AnimVal.Value;
                //destRect.Y = iElement.Y.AnimVal.Value;
                //destRect.Width = width;
                //destRect.Height = height;
                
                Rectangle rectangle = new Rectangle();
                if (!String.IsNullOrEmpty(this.Element.Id))
                    rectangle.Name = this.Element.Id;
                rectangle.Width = width;
                rectangle.Height = height;
                Canvas.SetTop(rectangle, iElement.Y.AnimVal.Value);
                Canvas.SetLeft(rectangle, iElement.X.AnimVal.Value);
                rectangle.Opacity = opacity;
                xamlRenderer.Canvas.Children.Add(rectangle);

                //Image image;
                if (iElement.IsSvgImage)
                {
                    if (!String.IsNullOrEmpty(iElement.Href.AnimVal))
                    {
                        SvgWindow wnd = getSvgWindow();
                        //gdiRenderer.BackColor = Color.Empty;
                        Canvas canvas = gdiRenderer.Render(wnd.Document as SvgDocument);
                        VisualBrush vb = new VisualBrush(canvas);
                        //vb.Viewbox = destRect;
                        vb.Stretch = Stretch.Fill;
                        rectangle.Fill = vb;
                        //canvas.Opacity = opacity;
                        //Canvas.SetLeft(canvas, iElement.X.AnimVal.Value);
                        //Canvas.SetTop(canvas, iElement.Y.AnimVal.Value);

                        //wnd.Render();
                        //image = gdiRenderer.RasterImage;
                        //image.Save(@"c:\inlinesvg.png", ImageFormat.Png);
                    }
                }
                else
                {
                    ImageBrush imgBrush = new ImageBrush(iElement.Bitmap);
                    imgBrush.Stretch = Stretch.Fill;
                    rectangle.Fill = imgBrush;
                    //image = iElement.Bitmap;
                }

                //if (image != null)
                //{
                //    graphics.DrawImage(image, destRect, 0f, 0f, image.Width, image.Height, GraphicsUnit.Pixel, imageAttributes);
                //}
            }
		}
        #endregion
	}
}
