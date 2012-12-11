using System;

namespace SharpVectors.Dom.Svg
{
	public class SvgSymbolElement : SvgStyleableElement, ISvgSymbolElement, ISharpDoNotPaint
	{
		#region Constructors
		internal SvgSymbolElement(string prefix, string localname, string ns, SvgDocument doc) : base(prefix, localname, ns, doc) 
		{
			svgExternalResourcesRequired = new SvgExternalResourcesRequired(this);
			svgFitToViewBox = new SvgFitToViewBox(this);
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
	}
}
