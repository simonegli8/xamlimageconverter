namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// Interface for matrix operations used within the SVG DOM
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <developer>kevin@kevlindev.com</developer>
	/// <completed>100</completed>
	public interface ISvgMatrix
	{
		double A { get; }
		double B { get; }
		double C { get; }
		double D { get; }
		double E { get; }
		double F { get; }
		
		ISvgMatrix Multiply(ISvgMatrix secondMatrix);
		ISvgMatrix Inverse();
		ISvgMatrix Translate(double x, double y);
		ISvgMatrix Scale(double scaleFactor);
		ISvgMatrix ScaleNonUniform(double scaleFactorX, double scaleFactorY);
		ISvgMatrix Rotate(double angle);
		ISvgMatrix RotateFromVector(double x, double y);
		ISvgMatrix FlipX();
		ISvgMatrix FlipY();
		ISvgMatrix SkewX(double angle);
		ISvgMatrix SkewY(double angle);
	}	
}
