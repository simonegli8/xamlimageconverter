using System;

using SharpVectors.Dom.Svg;

namespace SharpVectors.Enumerators
{
	/// <summary>
	/// Summary description for SvgLengthListEnumerator.
	/// </summary>
    public class SvgPointListEnumerator : SvgListEnumerator
    {
        public new SvgPoint Current 
        {
            get { return (SvgPoint) base.Current; }
        }

		public SvgPointListEnumerator(SvgPointList list) : base(list)
		{
		}
	}
}
