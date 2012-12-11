namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// This interface defines a list of SvgLength objects. 
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>20</completed>
	public interface ISvgLengthList
	{
		int NumberOfItems{get;}
		void Clear();
		ISvgLength Initialize(ISvgLength newItem);
		ISvgLength GetItem(int index);
		ISvgLength InsertItemBefore(ISvgLength newItem, int index);
		ISvgLength ReplaceItem(ISvgLength newItem, int index);
		ISvgLength RemoveItem(int index);
		ISvgLength AppendItem(ISvgLength newItem);
                        
        // not part of the SVG spec
        void FromString(string listString);
	}
}
