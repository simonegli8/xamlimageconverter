using System;
using System.IO;
using System.Xml;
using SharpVectors.Dom.Svg;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;

namespace SharpVectors.Renderer.Xaml
{
	public class PatternPaintServer : PaintServer
	{
		public PatternPaintServer(XamlRenderer renderer, SvgPatternElement patternElement)
            : base(renderer)
		{
			_patternElement = patternElement;
		}

		private SvgPatternElement _patternElement;

		#region Private methods
		private XmlElement oldParent;
		private SvgSvgElement moveIntoSvgElement()
		{
            SvgDocument doc = _patternElement.OwnerDocument;
			SvgSvgElement svgElm = doc.CreateElement("", "svg", SvgDocument.SvgNamespace) as SvgSvgElement;

			XmlNodeList children = _patternElement.Children;
			if(children.Count > 0)
			{
				oldParent = children[0].ParentNode as XmlElement;
			}

			for(int i = 0; i<children.Count; i++)
			{
				svgElm.AppendChild(children[i]);
			}

			if(_patternElement.HasAttribute("viewBox"))
			{
				svgElm.SetAttribute("viewBox", _patternElement.GetAttribute("viewBox"));
			}
			svgElm.SetAttribute("x", "0");
			svgElm.SetAttribute("y", "0");
			svgElm.SetAttribute("width", _patternElement.GetAttribute("width"));
			svgElm.SetAttribute("height", _patternElement.GetAttribute("height"));

			if(_patternElement.PatternContentUnits.AnimVal.Equals(SvgUnitType.ObjectBoundingBox))
			{
				svgElm.SetAttribute("viewBox", "0 0 1 1");
			}

			_patternElement.AppendChild(svgElm);

            return svgElm;
		}

		private void moveOutOfSvgElement(SvgSvgElement svgElm)
		{
			while(svgElm.ChildNodes.Count > 0)
			{
				oldParent.AppendChild(svgElm.ChildNodes[0]);
			}

			_patternElement.RemoveChild(svgElm);
		}

        private Canvas getImage(Rect bounds)
		{
            XamlRenderer renderer = new XamlRenderer();
			renderer.Window = _patternElement.OwnerDocument.Window as SvgWindow;

			SvgSvgElement elm = moveIntoSvgElement();

			Canvas img = renderer.Render(elm as SvgElement);

			moveOutOfSvgElement(elm);

			return img;
		}


		private double calcPatternUnit(SvgLength length, SvgLengthDirection dir, Rect bounds)
		{
			if(_patternElement.PatternUnits.AnimVal.Equals(SvgUnitType.UserSpaceOnUse))
			{
                return (double)length.Value;
			}
			else
			{
                double calcValue = (double)length.ValueInSpecifiedUnits;
				if(dir == SvgLengthDirection.Horizontal)
				{
					calcValue *= bounds.Width;
				}
				else
				{
					calcValue *= bounds.Height;
				}
				if(length.UnitType == SvgLengthType.Percentage)
				{
					calcValue /= 100F;
				}
				return calcValue;
			}
		}

		private Rect getDestRect(Rect bounds)
		{
			Rect result = new Rect(0, 0, 0, 0);
			result.Width = calcPatternUnit(_patternElement.Width.AnimVal as SvgLength, SvgLengthDirection.Horizontal, bounds);
			result.Height = calcPatternUnit(_patternElement.Height.AnimVal as SvgLength, SvgLengthDirection.Vertical, bounds);
			
			return result;
		}

		private Matrix getTransformMatrix(Rect bounds)
		{
			SvgMatrix svgMatrix = ((SvgTransformList)_patternElement.PatternTransform.AnimVal).TotalMatrix;

			Matrix transformMatrix = new Matrix(
				(double) svgMatrix.A,
				(double) svgMatrix.B,
				(double) svgMatrix.C,
				(double) svgMatrix.D,
				(double) svgMatrix.E,
				(double) svgMatrix.F);

			double translateX = calcPatternUnit(_patternElement.X.AnimVal as SvgLength, SvgLengthDirection.Horizontal, bounds);
			double translateY = calcPatternUnit(_patternElement.Y.AnimVal as SvgLength, SvgLengthDirection.Vertical, bounds);

			transformMatrix.Translate(translateX, translateY); //, MatrixOrder.Prepend);
			return transformMatrix;
		}
		#endregion

		#region Public methods
		public override Brush GetBrush(Rect bounds)
		{
			Canvas image = getImage(bounds);
            VisualBrush brush = new VisualBrush(image);
            brush.Transform = new MatrixTransform(getTransformMatrix(bounds));
            brush.TileMode = TileMode.None;
            //brush.Viewbox = getDestRect(bounds);

            //Rect destRect = getDestRect(bounds);

            //TextureBrush tb = new TextureBrush(image, destRect);
            //tb.Transform = getTransformMatrix(bounds);
            return brush;
		}
		#endregion
	}
}
