namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// Many of the SVG DOM interfaces refer to objects of class SvgPoint.
	/// An SvgPoint is an (x,y) coordinate pair. When used in matrix
	/// operations, an SvgPoint is treated as a vector of the form:
	/// 	[x]
	/// 	[y]
	/// 	[1]
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <developer>kevin@kevlindev.com</developer>
	/// <completed>100</completed>
	public interface ISvgPoint
	{
		double X { get; set; }
		double Y { get; set; }

		ISvgPoint MatrixTransform(ISvgMatrix matrix);
	}
}
