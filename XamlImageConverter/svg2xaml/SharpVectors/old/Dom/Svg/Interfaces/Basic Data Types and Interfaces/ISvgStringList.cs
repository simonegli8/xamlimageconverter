namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// This interface defines a list of string objects. 
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <developer>kevin@kevlindev.com</developer>
	/// <completed>100</completed>
	public interface ISvgStringList
	{
		int NumberOfItems{get;}
		void Clear();
		string Initialize(string newItem);
		string GetItem(int index);
		string InsertItemBefore(string newItem, int index);
		string ReplaceItem(string newItem, int index);
		string RemoveItem(int index);
		string AppendItem(string newItem);
                
        // not part of the SVG spec
        void FromString(string listString);
	}
}
