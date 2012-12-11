using System;

using SharpVectors.Dom.Svg;

namespace SharpVectors.Enumerators
{
	/// <summary>
	/// Summary description for SvgLengthListEnumerator.
	/// </summary>
    public class SvgLengthListEnumerator : SvgListEnumerator
    {
        public new SvgLength Current 
        {
            get { return (SvgLength) base.Current; }
        }

		public SvgLengthListEnumerator(SvgLengthList list) : base(list)
		{
		}
	}
}
