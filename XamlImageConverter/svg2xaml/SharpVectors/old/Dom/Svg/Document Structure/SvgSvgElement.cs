using System;
using System.Xml;

namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// A key interface definition is the SVGSVGElement interface, which is the interface that corresponds to the 'svg' element. This interface contains various miscellaneous commonly-used utility methods, such as matrix operations and the ability to control the time of redraw on visual rendering devices.
	/// SVGSVGElement extends ViewCSS and DocumentCSS to provide access to the computed values of properties and the override style sheet as described in DOM2. 
	/// </summary>
	public class SvgSvgElement : SvgTransformableElement, ISvgSvgElement
	{
		#region Constructors
		internal SvgSvgElement(string prefix, string localname, string ns, SvgDocument doc) : base(prefix, localname, ns, doc) 
		{
			svgExternalResourcesRequired = new SvgExternalResourcesRequired(this);
			svgFitToViewBox = new SvgFitToViewBox(this);
			svgTests = new SvgTests(this);
		}
		#endregion

		#region Implementation of ISvgZoomAndPan
		public SvgZoomAndPanType ZoomAndPan
		{
			get
			{
				throw new NotImplementedException();
			}
		}
		#endregion

		#region Implementation of ISvgSvgElement
		/// <summary>
		/// Corresponds to attribute x on the given 'svg' element.
		/// </summary>
		public ISvgAnimatedLength X
		{
			get
			{
				return new SvgAnimatedLength("x", this.GetAttribute("x"), "0px", this, SvgLengthDirection.Horizontal);
			}
		}
		/// <summary>
		/// Corresponds to attribute y on the given 'svg' element.
		/// </summary>
		public ISvgAnimatedLength Y
		{
			get
			{
				return new SvgAnimatedLength("y", this.GetAttribute("y"), "0px", this, SvgLengthDirection.Vertical);
			}
		}


		private string WidthAsString
		{
			get
			{
				SvgWindow ownerWindow = (SvgWindow)((SvgDocument)OwnerDocument).Window;
				if(ownerWindow.ParentWindow == null)
				{
					return GetAttribute("width").Trim();
				}
				else
				{
					return String.Empty;
				}
			}
		}

		/// <summary>
		/// Corresponds to attribute width on the given 'svg' element.
		/// </summary>
		public ISvgAnimatedLength Width
		{
			get
			{
				return new SvgAnimatedLength("width", WidthAsString, "100%", this, SvgLengthDirection.Horizontal);
			}
		}


		private string HeightAsString
		{
			get
			{
				SvgWindow ownerWindow = (SvgWindow)((SvgDocument)OwnerDocument).Window;
				if(ownerWindow.ParentWindow == null)
				{
					return GetAttribute("height").Trim();
				}
				else
				{
					return "";
				}
			}
		}
		/// <summary>
		/// Corresponds to attribute height on the given 'svg' element.
		/// </summary>
		public ISvgAnimatedLength Height
		{
			get
			{
				return new SvgAnimatedLength("height", HeightAsString, "100%", this, SvgLengthDirection.Vertical);
			}
		}		

		/// <summary>
		/// Corresponds to attribute contentScriptType on the given 'svg' element
		/// </summary>
		/// <exception cref="DomException">NO_MODIFICATION_ALLOWED_ERR: Raised on an attempt to change the value of a readonly attribute.</exception>
		public string ContentScriptType
		{
			get
			{
				return GetAttribute("contentScriptType");
			}
		}

		/// <summary>
		/// Corresponds to attribute contentStyleType on the given 'svg' element.
		/// </summary>
		/// <exception cref="DomException">NO_MODIFICATION_ALLOWED_ERR: Raised on an attempt to change the value of a readonly attribute.</exception>
		public string ContentStyleType
		{
			get
			{
				return GetAttribute("contentStyleType");
			}
		}
		 


		private double getViewportProp(string propertyName, string inValue, double calcParentVP, double defaultValue, SvgLengthDirection dir)
		{
			double ret;
			inValue = inValue.Trim();

			if(inValue.Length > 0)
			{
				if(inValue.EndsWith("%"))
				{
					double perc = SvgNumber.ParseToFloat(inValue.Substring(0, inValue.Length-1)) / 100;
					ret = calcParentVP * perc;
				}
				else
				{
					ret = new SvgLength(propertyName, inValue, this, dir).Value;
				}
			}
			else ret = defaultValue;

			return ret;
		}

		/// <summary>
		/// The position and size of the viewport (implicit or explicit) that corresponds to this 'svg' element. When the user agent is actually rendering the content, then the position and size values represent the actual values when rendering. The position and size values are unitless values in the coordinate system of the parent element. If no parent element exists (i.e., 'svg' element represents the root of the document tree), if this SVG document is embedded as part of another document (e.g., via the HTML 'object' element), then the position and size are unitless values in the coordinate system of the parent document. (If the parent uses CSS or XSL layout, then unitless values represent pixel units for the current CSS or XSL viewport, as described in the CSS2 specification.) If the parent element does not have a coordinate system, then the user agent should provide reasonable default values for this attribute.
		/// The object itself and its contents are both readonly.
		/// </summary>
		public ISvgRect Viewport
		{
			get
			{
				double calcParentVPWidth = (ViewportElement == null) ? 
					OwnerDocument.Window.InnerWidth : ((ISvgSvgElement)ViewportElement).Viewport.Width;

				double calcParentVPHeight = (ViewportElement == null) ? 
					OwnerDocument.Window.InnerHeight : ((ISvgSvgElement)ViewportElement).Viewport.Height;
				
				return new SvgRect(
                    getViewportProp(
						"x", 
					    GetAttribute("x"), 
					    (double) calcParentVPWidth, 
					    0, 
					    SvgLengthDirection.Horizontal),
				    getViewportProp(
						"y", 
					    GetAttribute("y"), 
					    (double) calcParentVPHeight, 
					    0, 
					    SvgLengthDirection.Vertical),
				    getViewportProp(
						"width", 
					    WidthAsString, 
					    (double) calcParentVPWidth, 
					    OwnerDocument.Window.InnerWidth, 
					    SvgLengthDirection.Horizontal),
				    getViewportProp(
						"height", 
					    HeightAsString, 
					    (double) calcParentVPHeight, 
					    OwnerDocument.Window.InnerHeight, 
					    SvgLengthDirection.Vertical)
                );
			}
		}

		/// <summary>
		/// Size of a pixel units (as defined by CSS2) adouble the x-axis of the viewport, which represents a unit somewhere in the range of 70dpi to 120dpi, and, on systems that support this, might actually match the characteristics of the target medium. On systems where it is impossible to know the size of a pixel, a suitable default pixel size is provided.
		/// </summary>
		public double PixelUnitToMillimeterX
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}
		
		/// <summary>
		/// Corresponding size of a pixel unit adouble the y-axis of the viewport.
		/// </summary>
		public double PixelUnitToMillimeterY
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// User interface (UI) events in DOM Level 2 indicate the screen positions at which the given UI event occurred. When the user agent actually knows the physical size of a "screen unit", this attribute will express that information; otherwise, user agents will provide a suitable default value such as .28mm.
		/// </summary>
		public double ScreenPixelToMillimeterX
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Corresponding size of a screen pixel adouble the y-axis of the viewport.
		/// </summary>
		public double ScreenPixelToMillimeterY
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// The initial view (i.e., before magnification and panning) of the current innermost SVG document fragment can be either the "standard" view (i.e., based on attributes on the 'svg' element such as fitBoxToViewport) or to a "custom" view (i.e., a hyperlink into a particular 'view' or other element - see Linking into SVG content: URI fragments and SVG views). If the initial view is the "standard" view, then this attribute is false. If the initial view is a "custom" view, then this attribute is true.
		/// </summary>
		/// <exception cref="DomException">NO_MODIFICATION_ALLOWED_ERR: Raised on an attempt to change the value of a readonly attribute.</exception>
		public bool UseCurrentView
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// The definition of the initial view (i.e., before magnification and panning) of the current innermost SVG document fragment. The meaning depends on the situation:
		/// * If the initial view was a "standard" view, then:
		///  o the values for viewBox, preserveAspectRatio and zoomAndPan within currentView will match the values for the corresponding DOM attributes that are on SVGSVGElement directly
		///  o the values for transform and viewTarget within currentView will be null
		/// * If the initial view was a link into a 'view' element, then:
		///  o the values for viewBox, preserveAspectRatio and zoomAndPan within currentView will correspond to the corresponding attributes for the given 'view' element
		///  o the values for transform and viewTarget within currentView will be null
		/// * If the initial view was a link into another element (i.e., other than a 'view'), then:
		///  o the values for viewBox, preserveAspectRatio and zoomAndPan within currentView will match the values for the corresponding DOM attributes that are on SVGSVGElement directly for the closest ancestor 'svg' element
		///  o the values for transform within currentView will be null
		///  o the viewTarget within currentView will represent the target of the link
		/// * If the initial view was a link into the SVG document fragment using an SVG view specification fragment identifier (i.e., #svgView(...)), then:
		///  o the values for viewBox, preserveAspectRatio, zoomAndPan, transform and viewTarget within currentView will correspond to the values from the SVG view specification fragment identifier
		/// The object itself and its contents are both readonly. 
		/// </summary>
		public ISvgViewSpec CurrentView
		{
			get
			{
				throw new NotImplementedException();
			}
    		
		}


		/// <summary>
		/// This attribute indicates the current scale factor relative to the initial view to take into account user magnification and panning operations, as described under Magnification and panning. DOM attributes currentScale and currentTranslate are equivalent to the 2x3 matrix [a b c d e f] = [currentScale 0 0 currentScale currentTranslate.x currentTranslate.y]. If "magnification" is enabled (i.e., zoomAndPan="magnify"), then the effect is as if an extra transformation were placed at the outermost level on the SVG document fragment (i.e., outside the outermost 'svg' element).
		/// </summary>
		/// <exception cref="DomException">NO_MODIFICATION_ALLOWED_ERR: Raised on an attempt to change the value of a readonly attribute</exception>
		public double CurrentScale
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// The corresponding translation factor that takes into account user "magnification".
		/// </summary>
		public ISvgPoint CurrentTranslate
		{
			get
			{
				throw new NotImplementedException();
			}
    		
		}

		/// <summary>
		/// Takes a time-out value which indicates that redraw shall not occur until: (a) the corresponding unsuspendRedraw(suspend_handle_id) call has been made, (b) an unsuspendRedrawAll() call has been made, or (c) its timer has timed out. In environments that do not support interactivity (e.g., print media), then redraw shall not be suspended. suspend_handle_id = suspendRedraw(max_wait_milliseconds) and unsuspendRedraw(suspend_handle_id) must be packaged as balanced pairs. When you want to suspend redraw actions as a collection of SVG DOM changes occur, then precede the changes to the SVG DOM with a method call similar to suspend_handle_id = suspendRedraw(max_wait_milliseconds) and follow the changes with a method call similar to unsuspendRedraw(suspend_handle_id). Note that multiple suspendRedraw calls can be used at once and that each such method call is treated independently of the other suspendRedraw method calls.
		/// </summary>
		/// <param name="max_wait_milliseconds">The amount of time in milliseconds to hold off before redrawing the device. Values greater than 60 seconds will be truncated down to 60 seconds.</param>
		/// <returns>A number which acts as a unique identifier for the given suspendRedraw() call. This value must be passed as the parameter to the corresponding unsuspendRedraw() method call.</returns>
		public int SuspendRedraw(int maxWaitMilliseconds)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Cancels a specified suspendRedraw() by providing a unique suspend_handle_id.
		/// </summary>
		/// <param name="suspend_handle_id">A number which acts as a unique identifier for the desired suspendRedraw() call. The number supplied must be a value returned from a previous call to suspendRedraw()</param>
		/// <exception cref="DomException">This method will raise a DOMException with value NOT_FOUND_ERR if an invalid value (i.e., no such suspend_handle_id is active) for suspend_handle_id is provided.</exception>
		public void UnsuspendRedraw(int suspendHandleId)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Cancels all currently active suspendRedraw() method calls. This method is most useful at the very end of a set of SVG DOM calls to ensure that all pending suspendRedraw() method calls have been cancelled.
		/// </summary>
		public void UnsuspendRedrawAll()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// In rendering environments supporting interactivity, forces the user agent to immediately redraw all regions of the viewport that require updating.
		/// </summary>
		public void ForceRedraw()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Suspends (i.e., pauses) all currently running animations that are defined within the SVG document fragment corresponding to this 'svg' element, causing the animation clock corresponding to this document fragment to stand still until it is unpaused.
		/// </summary>
		public void PauseAnimations()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Unsuspends (i.e., unpauses) currently running animations that are defined within the SVG document fragment, causing the animation clock to continue from the time at which it was suspended.
		/// </summary>
		public void UnpauseAnimations()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns true if this SVG document fragment is in a paused state
		/// </summary>
		/// <returns>Boolean indicating whether this SVG document fragment is in a paused state.</returns>
		public bool AnimationsPaused()
		{
			throw new NotImplementedException();
		}

		
		/// <summary>
		/// Returns the current time in seconds relative to the start time for the current SVG document fragment.
		/// </summary>
		/// <returns>The current time in seconds.</returns>
		public double GetCurrentTime()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Adjusts the clock for this SVG document fragment, establishing a new current time.
		/// </summary>
		/// <param name="seconds">The new current time in seconds relative to the start time for the current SVG document fragment.</param>
		public void SetCurrentTime(double seconds)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns the list of graphics elements whose rendered content intersects the supplied rectangle, honoring the 'pointer-events' property value on each candidate graphics element.
		/// </summary>
		/// <param name="rect">The test rectangle. The values are in the initial coordinate system for the current 'svg' element.</param>
		/// <param name="referenceElement">If not null, then only return elements whose drawing order has them below the given reference element.</param>
		/// <returns>A list of Elements whose content intersects the supplied rectangle.</returns>
		public XmlNodeList GetIntersectionList(ISvgRect rect, ISvgElement referenceElement)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns the list of graphics elements whose rendered content is entirely contained within the supplied rectangle, honoring the 'pointer-events' property value on each candidate graphics element.
		/// </summary>
		/// <param name="rect">The test rectangle. The values are in the initial coordinate system for the current 'svg' element.</param>
		/// <param name="referenceElement">If not null, then only return elements whose drawing order has them below the given reference element.</param>
		/// <returns>A list of Elements whose content is enclosed by the supplied rectangle.</returns>
		public XmlNodeList GetEnclosureList(ISvgRect rect, ISvgElement referenceElement)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns true if the rendered content of the given element intersects the supplied rectangle, honoring the 'pointer-events' property value on each candidate graphics element.
		/// </summary>
		/// <param name="element">The element on which to perform the given test.</param>
		/// <param name="rect">The test rectangle. The values are in the initial coordinate system for the current 'svg' element.</param>
		/// <returns>True or false, depending on whether the given element intersects the supplied rectangle.</returns>
		public bool CheckIntersection(ISvgElement element, ISvgRect rect)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns true if the rendered content of the given element is entirely contained within the supplied rectangle, honoring the 'pointer-events' property value on each candidate graphics element.
		/// </summary>
		/// <param name="element">The element on which to perform the given test</param>
		/// <param name="rect">The test rectangle. The values are in the initial coordinate system for the current 'svg' element.</param>
		/// <returns>True or false, depending on whether the given element is enclosed by the supplied rectangle.</returns>
		public bool CheckEnclosure(ISvgElement element, ISvgRect rect)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Unselects any selected objects, including any selections of text strings and type-in bars.
		/// </summary>
		public void DeselectAll()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Creates an SVGNumber object outside of any document trees. The object is initialized to a value of zero.
		/// </summary>
		/// <returns>An SVGNumber object.</returns>
		public double CreateSvgNumber()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Creates an SVGLength object outside of any document trees. The object is initialized to the value of 0 user units.
		/// </summary>
		/// <returns>An SVGLength object.</returns>
		public ISvgLength CreateSvgLength()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Creates an SVGAngle object outside of any document trees. The object is initialized to the value 0 degrees (unitless).
		/// </summary>
		/// <returns>An SVGAngle object.</returns>
		public ISvgAngle CreateSvgAngle()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Creates an SVGPoint object outside of any document trees. The object is initialized to the point (0,0) in the user coordinate system.
		/// </summary>
		/// <returns>An SVGPoint object.</returns>
		public ISvgPoint CreateSvgPoint()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Creates an SVGMatrix object outside of any document trees. The object is initialized to the identity matrix.
		/// </summary>
		/// <returns>An SVGMatrix object.</returns>
		public ISvgMatrix CreateSvgMatrix()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Creates an SVGRect object outside of any document trees. The object is initialized such that all values are set to 0 user units.
		/// </summary>
		/// <returns>An SVGRect object.</returns>
		public ISvgRect CreateSvgRect()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Creates an SVGTransform object outside of any document trees. The object is initialized to an identity matrix transform (SVG_TRANSFORM_MATRIX).
		/// </summary>
		/// <returns>An SVGTransform object.</returns>
		public ISvgTransform CreateSvgTransform()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Creates an SVGTransform object outside of any document trees. The object is initialized to the given matrix transform (i.e., SVG_TRANSFORM_MATRIX).
		/// </summary>
		/// <param name="matrix">The transform matrix.</param>
		/// <returns>An SVGTransform object.</returns>
		public ISvgTransform CreateSvgTransformFromMatrix(ISvgMatrix matrix)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Searches this SVG document fragment (i.e., the search is restricted to a subset of the document tree) for an Element whose id is given by elementId. If an Element is found, that Element is returned. If no such element exists, returns null. Behavior is not defined if more than one element has this id.
		/// </summary>
		/// <param name="elementId">The unique id value for an element.</param>
		/// <returns>The matching element.</returns>
		public XmlElement GetElementById(string elementId)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Implementation of ISvgFitToViewBox
		private SvgFitToViewBox svgFitToViewBox;
		public ISvgAnimatedRect ViewBox
		{
			get
			{
				return svgFitToViewBox.ViewBox;
			}
		}

		public ISvgAnimatedPreserveAspectRatio PreserveAspectRatio
		{
			get
			{
				return svgFitToViewBox.PreserveAspectRatio;
			}
		}
		#endregion

		#region Implementation of ISvgExternalResourcesRequired
		private SvgExternalResourcesRequired svgExternalResourcesRequired;
		public ISvgAnimatedBoolean ExternalResourcesRequired
		{
			get
			{
				return svgExternalResourcesRequired.ExternalResourcesRequired;
			}
		}
		#endregion

		#region Implementation of ISvgTests
		private SvgTests svgTests;
		public ISvgStringList RequiredFeatures
		{
			get { return svgTests.RequiredFeatures; }
		}

		public ISvgStringList RequiredExtensions
		{
			get { return svgTests.RequiredExtensions; }
		}

		public ISvgStringList SystemLanguage
		{
			get { return svgTests.SystemLanguage; }
		}

		public bool HasExtension(string extension)
		{
			return svgTests.HasExtension(extension);
		}
        #endregion
	}
}
