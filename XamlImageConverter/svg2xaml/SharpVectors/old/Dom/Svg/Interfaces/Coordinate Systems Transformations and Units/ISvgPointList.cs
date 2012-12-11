using System;

namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// This interface defines a list of SvgPoint objects. 
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <developer>kevin@kevlindev.com</developer>
	/// <completed>60</completed>
	public interface ISvgPointList
	{
		int NumberOfItems{get;}
		void Clear();
		ISvgPoint Initialize(ISvgPoint newItem);
		ISvgPoint GetItem(int index);
		ISvgPoint InsertItemBefore(ISvgPoint newItem, int index);
		ISvgPoint ReplaceItem(ISvgPoint newItem, int index);
		ISvgPoint RemoveItem(int index);
		ISvgPoint AppendItem(ISvgPoint newItem);
                        
        // not part of the SVG spec
        void FromString(string listString);
	}
}
