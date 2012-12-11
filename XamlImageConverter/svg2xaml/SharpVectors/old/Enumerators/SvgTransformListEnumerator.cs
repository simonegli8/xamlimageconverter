using System;

using SharpVectors.Dom.Svg;

namespace SharpVectors.Enumerators
{
	/// <summary>
	/// Summary description for SvgLengthListEnumerator.
	/// </summary>
    public class SvgTransformListEnumerator : SvgListEnumerator
    {
        public new SvgTransform Current 
        {
            get { return (SvgTransform) base.Current; }
        }

		public SvgTransformListEnumerator(SvgTransformList list) : base(list)
		{
		}
	}
}
