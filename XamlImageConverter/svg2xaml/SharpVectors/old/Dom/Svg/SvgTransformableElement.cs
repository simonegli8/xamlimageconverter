using System;
using System.Xml;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows;


namespace SharpVectors.Dom.Svg
{
	public /*abstract*/ class SvgTransformableElement : SvgStyleableElement, ISvgTransformable
	{
		#region Constructors
		internal SvgTransformableElement(string prefix, string localname, string ns, SvgDocument doc)
			: base(prefix, localname, ns, doc)
		{
		}

		#endregion

		#region Implementation of ISvgTransformable
		public ISvgAnimatedTransformList Transform
		{
			get
			{
				return (ISvgAnimatedTransformList) new SvgAnimatedTransformList(GetAttribute("transform"));	
			}
		}

		public ISvgElement NearestViewportElement
		{
			get
			{
				XmlNode parent = this.ParentNode;
				while(parent != null)
				{
					if(parent is SvgSvgElement)
					{
						return (SvgElement)parent;
					}
					parent = parent.ParentNode;
				}
				return null;
			}
		}
		public ISvgElement FarthestViewportElement
		{
			get
			{
				SvgDocument doc = (SvgDocument) this.OwnerDocument;
				if(doc.RootElement == this) return null;
				else return (SvgElement) doc.RootElement;
			}
		}

		public ISvgRect GetBBox()
		{
			if(this is ISharpPathGeometry)
			{
				ISharpPathGeometry gdiPathElm = (ISharpPathGeometry) this;
				Geometry _gp = gdiPathElm.GetGeometry();

                Transform oldTransform = _gp.Transform;                

				SvgMatrix svgMatrix = (SvgMatrix)this.GetScreenCTM();
                Matrix matrix = new Matrix(
                    (double) svgMatrix.A,
                    (double) svgMatrix.B,
                    (double) svgMatrix.C,
                    (double) svgMatrix.D,
                    (double) svgMatrix.E,
                    (double) svgMatrix.F);
                _gp.Transform = new MatrixTransform(matrix);
                Rect bounds = _gp.Bounds;
                _gp.Transform = oldTransform;

				return new SvgRect(bounds.X, bounds.Y, bounds.Width, bounds.Height);
			}
			else
			{
				Rect union = Rect.Empty;
				SvgTransformableElement transformChild;

				foreach(XmlNode childNode in ChildNodes)
				{
					if(childNode is SvgTransformableElement)
					{
						transformChild = (SvgTransformableElement) childNode;
						SvgRect svgBBox = (SvgRect)transformChild.GetBBox();
						Rect bbox = new Rect(
                            (double) svgBBox.X,
                            (double) svgBBox.Y,
                            (double) svgBBox.Width,
                            (double) svgBBox.Height
                        );
						if(union == Rect.Empty) union = bbox;
						else union = Rect.Union(union, bbox);
					}
				}
				
				return new SvgRect(union.X, union.Y, union.Width, union.Height);
			}
		}

		public ISvgMatrix GetCTM()
		{
			ISvgElement nVE = this.FarthestViewportElement;
			SvgMatrix matrix = new SvgMatrix();
			if(nVE != null)
			{
				SvgTransformList svgTList = (SvgTransformList)this.Transform.AnimVal;
				matrix = (SvgMatrix)svgTList.Consolidate().Matrix;
				SvgTransformableElement par = (SvgTransformableElement) ParentNode;
				while(par != nVE)
				{
					svgTList = (SvgTransformList)par.Transform.AnimVal;
					matrix = (SvgMatrix)matrix.Multiply(svgTList.Consolidate().Matrix);
					par = (SvgTransformableElement) par.ParentNode;
				}
			}
			return matrix;
		}

		public ISvgMatrix GetScreenCTM()
		{
			ISvgElement fVE = this.FarthestViewportElement;
			SvgMatrix matrix = new SvgMatrix();

			if(fVE == null)
			{
				SvgTransformList svgTList = (SvgTransformList)this.Transform.AnimVal;
				matrix = (SvgMatrix)svgTList.Consolidate().Matrix;
				SvgTransformableElement par = (SvgTransformableElement) ParentNode;
				while(par != fVE)
				{
					svgTList = (SvgTransformList)par.Transform.AnimVal;
					matrix = (SvgMatrix)matrix.Multiply(svgTList.Consolidate().Matrix);
					par = (SvgTransformableElement) par.ParentNode;
				}
			}
			return matrix;
		}

		public ISvgMatrix GetTransformToElement(ISvgElement element)
		{
			throw new NotImplementedException("getTransformToElement()");
			//raises( SvgException );
		}

		#endregion
	}
}
