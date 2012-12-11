using System;
using System.Text.RegularExpressions;

namespace SharpVectors.Dom.Svg
{
    /// <summary>
    /// This interface defines a list of SvgLength objects
    /// </summary>
    /// <developer>niklas@protocol7.com</developer>
    /// <developer>kevin@kevlindev.com.com</developer>
    /// <completed>100</completed>
	public class SvgLengthList : SvgList, ISvgLengthList
	{
        #region Fields
        private SvgElement ownerElement;
        private SvgLengthDirection direction;
		private string propertyName;
        #endregion

        #region Constructors
        public SvgLengthList()
        {
        }

        public SvgLengthList(string listString)
        {
            this.FromString(listString);
        }

        public SvgLengthList(string propertyName, string listString, SvgElement ownerElement, SvgLengthDirection direction)
        {
			this.propertyName = propertyName;
			this.ownerElement = ownerElement;
			this.direction = direction;

			this.FromString(listString);
        }
        #endregion

        #region ISvgLengthList Interface
        public ISvgLength Initialize(ISvgLength newItem)
        {
            return (ISvgLength) base.Initialize(newItem);
        }

        public new ISvgLength GetItem(int index)
        {
            return (ISvgLength) base.GetItem(index);
        }

        public ISvgLength InsertItemBefore(ISvgLength newItem, int index)
        {
            return (ISvgLength) base.InsertItemBefore(newItem, index);
        }

        public ISvgLength ReplaceItem(ISvgLength newItem, int index)
        {
            return (ISvgLength) base.ReplaceItem(newItem, index);
        }

        public new ISvgLength RemoveItem(int index)
        {
            return (ISvgLength) base.RemoveItem(index);
        }

        public ISvgLength AppendItem(ISvgLength newItem)
        {
            return (ISvgLength) base.AppendItem(newItem);
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

                        AppendItem( new SvgLength(propertyName, item, ownerElement, direction) );
                    }
                }
            }
        }
	}
}
