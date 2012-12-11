using System;

using SharpVectors.Dom.Svg;

namespace SharpVectors.Enumerators
{
	/// <summary>
	/// Summary description for SvgLengthListEnumerator.
	/// </summary>
    public class SvgNumberListEnumerator : SvgListEnumerator
    {
        public new SvgNumber Current 
        {
            get { return (SvgNumber) base.Current; }
        }

		public SvgNumberListEnumerator(SvgNumberList list) : base(list)
		{
		}
	}
}
