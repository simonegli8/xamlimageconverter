using System;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;


namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// This is an extension to the Svg DOM. Denotes that an element has a drawable shape.
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>50</completed>
	public interface ISharpShape
	{
		Shape GetShape();
		void Invalidate();
	}
}
