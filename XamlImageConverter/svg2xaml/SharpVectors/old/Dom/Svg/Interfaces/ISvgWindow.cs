using System;
using SharpVectors.Dom.Stylesheets;
using System.Xml;

namespace SharpVectors.Dom.Svg
{
	public interface ISvgWindow
	{
		IStyleSheet DefaultStyleSheet{get;}
		ISvgDocument Document{get;}
		/*Event Evt{get;}*/
		double InnerHeight{get;}
		double InnerWidth{get;}
		string Src{get;set;}

		void ClearInterval (object interval);
		void ClearTimeout (object timeout);
		/*void GetURL (string uri, EventListener callback);	*/
		XmlDocumentFragment ParseXML (string source, XmlDocument document);
		/*void PostURL (string uri, string data, EventListener callback, string mimeType, string contentEncoding);*/
		string PrintNode (XmlNode node);
		object SetInterval (string code, double delay);
		object SetTimeout (string code, double delay);
	}
}
