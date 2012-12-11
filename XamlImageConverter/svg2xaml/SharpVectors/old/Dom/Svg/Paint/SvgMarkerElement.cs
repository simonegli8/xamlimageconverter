using System;

namespace SharpVectors.Dom.Svg
{
	public class SvgMarkerElement : SvgStyleableElement, ISvgMarkerElement, ISharpDoNotPaint
	{
		internal SvgMarkerElement(string prefix, string localname, string ns, SvgDocument doc) : base(prefix, localname, ns, doc) 
		{
			svgExternalResourcesRequired = new SvgExternalResourcesRequired(this);
			svgFitToViewBox = new SvgFitToViewBox(this);
		}

		#region Implementation of ISvgMarkerElement
		/// <summary>
		///  Sets the value of attribute orient to 'auto'.
		/// </summary>
		public void SetOrientToAuto()
		{
			throw new NotImplementedException("SetOrientToAuto");
		}

		/// <summary>
		///  Sets the value of attribute orient to the given angle.
		/// </summary>
		/// <param name="angle"> The angle value to use for attribute orient.</param>
		public void SetOrientToAngle(SharpVectors.Dom.Svg.ISvgAngle angle)
		{
			throw new NotImplementedException("SetOrientToAngle");
		}

		/// <summary>
		/// Corresponds to attribute refX on the given 'marker' element.
		/// </summary>
		public SharpVectors.Dom.Svg.ISvgAnimatedLength RefX
		{
			get
			{
				return new SvgAnimatedLength("refX", GetAttribute("refX"), "0", this, SvgLengthDirection.Horizontal);
			}
		}

		/// <summary>
		/// Corresponds to attribute refY on the given 'marker' element.
		/// </summary>
		public SharpVectors.Dom.Svg.ISvgAnimatedLength RefY
		{
			get
			{
				return new SvgAnimatedLength("refY", GetAttribute("refY"), "0", this, SvgLengthDirection.Vertical);
			}
		}

		/// <summary>
		/// Corresponds to attribute markerUnits on the given 'marker' element.
		/// </summary>
		public SharpVectors.Dom.Svg.ISvgAnimatedEnumeration MarkerUnits
		{
			get
			{
				SvgMarkerUnit type = SvgMarkerUnit.Unknown;
				switch(GetAttribute("markerUnits"))
				{
					case "userSpaceOnUse":
						type = SvgMarkerUnit.UserSpaceOnUse;
						break;
					case "":
					case "strokeWidth":
						type = SvgMarkerUnit.StrokeWidth;
						break;
				}
				return new SvgAnimatedEnumeration(type);
			}
		}

		/// <summary>
		/// Corresponds to attribute markerWidth on the given 'marker' element
		/// </summary>
		public SharpVectors.Dom.Svg.ISvgAnimatedLength MarkerWidth
		{
			get
			{
				return new SvgAnimatedLength("markerWidth", GetAttribute("markerWidth"), "3", this, SvgLengthDirection.Horizontal);
			}
		}

		/// <summary>
		/// Corresponds to attribute markerHeight on the given 'marker' element.
		/// </summary>
		public SharpVectors.Dom.Svg.ISvgAnimatedLength MarkerHeight
		{
			get
			{
				return new SvgAnimatedLength("markerHeight", GetAttribute("markerHeight"), "3", this, SvgLengthDirection.Vertical);
			}
		}

		/// <summary>
		/// Corresponds to attribute orient on the given 'marker' element. One of the Marker Orientation Types defined above.
		/// </summary>
		public SharpVectors.Dom.Svg.ISvgAnimatedEnumeration OrientType
		{
			get
			{
				if(GetAttribute("orient") == "auto")
				{
					return new SvgAnimatedEnumeration(SvgMarkerOrient.Auto);
				}
				else
				{
					return new SvgAnimatedEnumeration(SvgMarkerOrient.Angle);
				}
			}
		}

		/// <summary>
		/// Corresponds to attribute orient on the given 'marker' element. If markerUnits is SVG_MARKER_ORIENT_ANGLE, the angle value for attribute orient; otherwise, it will be set to zero.
		/// </summary>
		public SharpVectors.Dom.Svg.ISvgAnimatedAngle OrientAngle
		{
			get
			{
				if(OrientType.AnimVal.Equals(SvgMarkerOrient.Angle))
				{
                    return new SvgAnimatedAngle(GetAttribute("orient"), "0");
				}
				else
				{
					return new SvgAnimatedAngle("0", "0");
				}
			}
		}
		#endregion

		#region Implementation of ISvgFitToViewBox
		private SvgFitToViewBox svgFitToViewBox;
		public ISvgAnimatedRect ViewBox
		{
			get
			{
				return svgFitToViewBox.ViewBox;
			}
		}

		public ISvgAnimatedPreserveAspectRatio PreserveAspectRatio
		{
			get
			{
				return svgFitToViewBox.PreserveAspectRatio;
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
	}
}
