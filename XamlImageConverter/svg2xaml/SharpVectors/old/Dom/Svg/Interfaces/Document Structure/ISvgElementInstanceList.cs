using System;

namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// The SvgElementInstanceList interface provides the abstraction of an ordered collection of SvgElementInstance objects, without defining or constraining how this collection is implemented.
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>0</completed>
	public interface ISvgElementInstanceList
	{
		double Length{get;}
		ISvgElementInstance Item ( double index );
	}
}
