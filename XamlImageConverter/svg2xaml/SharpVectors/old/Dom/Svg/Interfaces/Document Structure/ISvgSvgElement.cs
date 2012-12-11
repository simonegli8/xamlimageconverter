using System.Xml;

namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// A key interface definition is the SvgSvgElement interface, which is the interface that corresponds to the 'svg' element. This interface contains various miscellaneous commonly-used utility methods, such as matrix operations and the ability to control the time of redraw on visual rendering devices. 
	/// </summary>
	/// <developer>niklas@protocol7.com</developer>
	/// <completed>20</completed>
	public interface ISvgSvgElement	:
		ISvgElement,
		ISvgTests,
		ISvgLangSpace,
		ISvgExternalResourcesRequired,
		ISvgStylable,
		ISvgLocatable,
		ISvgFitToViewBox,
		ISvgZoomAndPan/*,
		org.w3c.dom.events.IEventTarget,
		org.w3c.dom.events.IDocumentEvent,
		org.w3c.dom.css.IViewCSS,
		org.w3c.dom.css.IDocumentCSS*/
	{

		ISvgAnimatedLength X{get;}
		ISvgAnimatedLength Y{get;}
		ISvgAnimatedLength Width{get;}
		ISvgAnimatedLength Height{get;}
		 
		string ContentScriptType{get;}
		string ContentStyleType{get;}
		 
		ISvgRect Viewport{get;}

		double PixelUnitToMillimeterX{get;}
		double PixelUnitToMillimeterY{get;}
		double ScreenPixelToMillimeterX{get;}
		double ScreenPixelToMillimeterY{get;}

		bool UseCurrentView{get;set;}
		ISvgViewSpec CurrentView{get;}

		double CurrentScale{get;set;}
		ISvgPoint CurrentTranslate{get;}

		int SuspendRedraw(int max_wait_milliseconds);
		void UnsuspendRedraw(int suspend_handle_id);
		void UnsuspendRedrawAll();
		void ForceRedraw();
		void PauseAnimations();
		void UnpauseAnimations();
		bool AnimationsPaused();
		double GetCurrentTime();

		XmlNodeList GetIntersectionList(ISvgRect rect, ISvgElement referenceElement);
		XmlNodeList GetEnclosureList(ISvgRect rect, ISvgElement referenceElement);
		bool CheckIntersection(ISvgElement element, ISvgRect rect);
		bool CheckEnclosure(ISvgElement element, ISvgRect rect);
		void DeselectAll();
		double CreateSvgNumber();
		ISvgLength CreateSvgLength();
		ISvgAngle CreateSvgAngle();
		ISvgPoint CreateSvgPoint();
		ISvgMatrix CreateSvgMatrix();
		ISvgRect CreateSvgRect();
		ISvgTransform CreateSvgTransform();
		ISvgTransform CreateSvgTransformFromMatrix(ISvgMatrix matrix);
		XmlElement GetElementById(string elementId);
	}
}
