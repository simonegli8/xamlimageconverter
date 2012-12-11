namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// 
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>90</completed>
	public interface ISvgDocument /*:
		IDocument,
		org.w3c.dom.events.IDocumentEvent*/
	{
		string Title{get;}
		string Referrer{get;}
		string Domain{get;}
		string Url{get;}
		ISvgSvgElement RootElement{get;}
		ISvgWindow Window{get;}
	}
}
