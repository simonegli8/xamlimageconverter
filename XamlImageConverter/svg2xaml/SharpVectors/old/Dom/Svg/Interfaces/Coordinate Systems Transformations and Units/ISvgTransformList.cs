namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// This interface defines a list of SvgTransform objects. 
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>50</completed>
	public interface ISvgTransformList
	{
		 int NumberOfItems { get; }
		 void Clear();
		 ISvgTransform Initialize(ISvgTransform newItem);
		 ISvgTransform GetItem(int index);
		 ISvgTransform InsertItemBefore(ISvgTransform newItem, int index);
		 ISvgTransform ReplaceItem(ISvgTransform newItem, int index);
		 ISvgTransform RemoveItem(int index);
		 ISvgTransform AppendItem(ISvgTransform newItem);
		 ISvgTransform CreateSvgTransformFromMatrix(ISvgMatrix matrix);
		 ISvgTransform Consolidate();
	}
}
