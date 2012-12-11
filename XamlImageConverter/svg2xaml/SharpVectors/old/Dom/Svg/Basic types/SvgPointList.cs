using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace SharpVectors.Dom.Svg
{
    /// <summary>
    /// This class defines a list of SvgPoint objects. 
    /// </summary>
    /// <developer>niklas@protocol7.com</developer>
    /// <developer>kevin@kevlindev.com</developer>
    /// <completed>100</completed>
    public class SvgPointList : SvgList, ISvgPointList
    {
        #region Constructors
        public SvgPointList()
        {

        }

        public SvgPointList(string listString)
        {
            this.FromString(listString);
        }
        #endregion

        #region ISvgStringList Interface
        public ISvgPoint Initialize(ISvgPoint newItem)
        {
            return (SvgPoint)base.Initialize(newItem);
        }

        public new ISvgPoint GetItem(int index)
        {
            return (SvgPoint)base.GetItem(index);
        }

        public ISvgPoint InsertItemBefore(ISvgPoint newItem, int index)
        {
            return (SvgPoint)base.InsertItemBefore(newItem, index);
        }

        public ISvgPoint ReplaceItem(ISvgPoint newItem, int index)
        {
            return (SvgPoint)base.ReplaceItem(newItem, index);
        }

        public new ISvgPoint RemoveItem(int index)
        {
            return (SvgPoint)base.RemoveItem(index);
        }

        public ISvgPoint AppendItem(ISvgPoint newItem)
        {
            return (SvgPoint)base.AppendItem(newItem);
        }
        #endregion

        public void FromString(string listString)
        {
            // remove existing list items
            Clear();

            if (listString != null)
            {
                // remove leading and trailing whitespace
                // NOTE: Need to check if .NET whitespace = SVG (XML) whitespace
                listString = listString.Trim();

                if (listString.Length > 0)
                {
                    List<string> coords = new List<string>();
                    foreach (Match m in  SvgNumber.DoubleRegex.Matches(listString))
                    {
                        if (!string.IsNullOrEmpty(m.Value))
                            coords.Add(m.Value);
                    }
                    //String[] coords = delim.Split(listString);

                    if (coords.Count % 2 == 1)
                        throw new SvgException(SvgExceptionType.SvgInvalidValueErr);

                    for (int i = 0; i < coords.Count; i += 2)
                    {
                        string x = coords[i];
                        string y = coords[i + 1];

                        if (x.Length == 0 || y.Length == 0)
                            throw new SvgException(SvgExceptionType.SvgInvalidValueErr);

                        AppendItem(new SvgPoint((double)SvgNumber.ParseToFloat(x), (double)SvgNumber.ParseToFloat(y)));
                    }
                }
            }
        }
    }
}
