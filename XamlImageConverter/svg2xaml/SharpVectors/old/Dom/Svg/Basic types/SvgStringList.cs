using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

using SharpVectors.Dom;

namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// This interface defines a list of String objects
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <developer>kevin@kevlindev.com.com</developer>
	/// <completed>100</completed>
	public class SvgStringList : SvgList, ISvgStringList
	{
        #region Constructors
        public SvgStringList()
        {
        }

        public SvgStringList(string listString)
        {
            this.FromString(listString);
        }
        #endregion

        #region ISvgStringList Interface
		public string Initialize(string newItem)
		{
            return (string) base.Initialize(newItem);
		}

		public new string GetItem(int index)
		{
			return (string) base.GetItem(index);
		}

		public string InsertItemBefore(string newItem, int index)
		{
			return (string) base.InsertItemBefore(newItem, index);
		}

		public string ReplaceItem(string newItem, int index)
		{
			return (string) base.ReplaceItem(newItem, index);
		}

		public new string RemoveItem(int index)
		{
			return (string) base.RemoveItem(index);
		}

		public string AppendItem(string newItem)
		{
			return (string) base.AppendItem(newItem);
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

                        AppendItem(item);
                    }
                }
            }
		}
	}
}
