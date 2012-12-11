using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

using SharpVectors.Dom;

namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// Summary description for SvgNumberList.
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <developer>kevin@kevlindev.com</developer>
	/// <completed>100</completed>
	public class SvgNumberList : SvgList, ISvgNumberList
	{
        #region Constructors
        public SvgNumberList()
        {
        }

        public SvgNumberList(string listString)
        {
            this.FromString(listString);
        }
        #endregion

		#region ISvgNumberList Interface
		public ISvgNumber Initialize(ISvgNumber newItem)
		{
            return (ISvgNumber) base.Initialize(newItem);
		}

		public new ISvgNumber GetItem(int index)
		{
			return (ISvgNumber) base.GetItem(index);
		}

		public ISvgNumber InsertItemBefore(ISvgNumber newItem, int index)
		{
			return (ISvgNumber) base.InsertItemBefore(newItem, index);
		}

		public ISvgNumber ReplaceItem(ISvgNumber newItem, int index)
		{
			return (ISvgNumber) base.ReplaceItem(newItem, index);
		}

		public new ISvgNumber RemoveItem(int index)
		{
			return (ISvgNumber) base.RemoveItem(index);
		}

		public ISvgNumber AppendItem(ISvgNumber newItem)
		{
			return (ISvgNumber) base.AppendItem(newItem);
		}
		#endregion

		public void FromString(string listString)
		{
            // remove existing list items
            Clear();

            if ( listString != null )
            {
                // remove leading and trailing whitespace
                // NOTE: Need to check if .NET whitespace = SVG (XML) whitespace
                listString = listString.Trim();

                if ( listString.Length > 0 )
                {
                    Regex delim = new Regex(@"\s+,?\s*|,\s*");
                    foreach ( string item in delim.Split(listString) )
                    {
                        // the following test is needed to catch consecutive commas
                        // for example, "one,two,,three"
                        if ( item.Length == 0 )
                            throw new DomException(DomExceptionType.SyntaxErr);

                        AppendItem(new SvgNumber(item));
                    }
                }
            }
		}
	}
}
