namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// This interface defines a all methods used in a Svg*List interface. 
	/// </summary>
	/// <developer>kevin@kevlidnev.com</developer>
	/// <completed>100</completed>
	public interface ISvgList
	{
		double NumberOfItems{get;}
		void Clear();
		object Initialize(object newItem);
		object GetItem(double index);
		object InsertItemBefore(object newItem, double index);
		object ReplaceItem(object newItem, double index);
		object RemoveItem(double index);
		object AppendItem(object newItem);
	}
}
