using System;

namespace SharpVectors.Dom.Css
{
	/// <summary>
	/// The Rect interface is used to represent any rect value. This interface reflects the values in the underlying style property. Hence, modifications made to the CSSPrimitiveValue objects modify the style property.
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>100</completed>
	public class Rect : IRect
	{
		#region Constructors
		/// <summary>
		/// Constructs a new Rect
		/// </summary>
		/// <param name="s">The string to parse that contains the Rect structure</param>
		/// <param name="readOnly">Specifies if the Rect should be read-only</param>
		public Rect(string s, bool readOnly)
		{
			this.readOnly = readOnly;

			string[] parts = s.Split(new char[]{' '});
			if(parts.Length == 4)
			{
				_top = new CssPrimitiveLengthValue(parts[0], readOnly);
				_right = new CssPrimitiveLengthValue(parts[1], readOnly);
				_bottom = new CssPrimitiveLengthValue(parts[2], readOnly);
				_left = new CssPrimitiveLengthValue(parts[3], readOnly);
			}
			else
			{
				throw new DomException(DomExceptionType.SyntaxErr);
			}
		}
		#endregion

		#region Private fields
		private bool readOnly;
		#endregion

		#region Implementation of IRect
		private CssPrimitiveValue _left;
		/// <summary>
		/// This attribute is used for the left of the rect.
		/// </summary>
		public ICssPrimitiveValue Left
		{
			get
			{
				return _left;
			}
		}

		private CssPrimitiveValue _bottom;
		/// <summary>
		/// This attribute is used for the bottom of the rect.
		/// </summary>
		public ICssPrimitiveValue Bottom
		{
			get
			{
				return _bottom;
			}
		}

		private CssPrimitiveValue _right;
		/// <summary>
		/// This attribute is used for the right of the rect.
		/// </summary>
		public ICssPrimitiveValue Right
		{
			get
			{
				return _right;
			}
		}

		private CssPrimitiveValue _top;
		/// <summary>
		/// This attribute is used for the top of the rect.
		/// </summary>
		public ICssPrimitiveValue Top
		{
			get
			{
				return _top;
			}
		}
		#endregion
	}
}
