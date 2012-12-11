using System;
using System.Windows.Media;
using System.Windows;


namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// This is an extension to the Svg DOM. Denotes that an element has a drawable shape.
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>50</completed>
	public interface ISharpPathGeometry
	{
		Geometry GetGeometry();
		void Invalidate();
	}
}
