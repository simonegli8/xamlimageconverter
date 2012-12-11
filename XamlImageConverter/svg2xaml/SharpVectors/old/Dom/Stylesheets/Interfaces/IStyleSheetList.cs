using System;

namespace SharpVectors.Dom.Stylesheets
{
	/// <summary>
	/// The StyleSheetList interface provides the abstraction of an
	/// ordered collection of style sheets. 
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>100</completed>
	/// 
	public interface IStyleSheetList
	{
		/// <summary>
		/// The number of StyleSheets in the list. The range of valid child stylesheet indices is 0 to length-1 inclusive.
		/// </summary>
		double Length{get;}

		/*IStyleSheet Item(double index);*/

		/// <summary>
		/// Used to retrieve a style sheet by ordinal index. If index is greater than or equal to the number of style sheets in the list, this returns null.
		/// </summary>
		IStyleSheet this[double index]{get;}
	}
}
