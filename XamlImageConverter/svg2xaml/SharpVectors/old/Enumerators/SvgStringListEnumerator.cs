using System;

using SharpVectors.Dom.Svg;

namespace SharpVectors.Enumerators
{
	/// <summary>
	/// Summary description for SvgLengthListEnumerator.
	/// </summary>
    public class SvgStringListEnumerator : SvgListEnumerator
    {
        public new string Current 
        {
            get { return (string) base.Current; }
        }

		public SvgStringListEnumerator(SvgStringList list) : base(list)
		{
		}
	}
}
