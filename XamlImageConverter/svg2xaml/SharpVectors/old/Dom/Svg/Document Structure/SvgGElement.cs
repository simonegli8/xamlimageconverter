using System;
using System.Xml;
using System.Windows.Media;
using System.Diagnostics;

using SharpVectors.Dom.Events;


namespace SharpVectors.Dom.Svg
{
	/// <summary>
	/// The SvgGElement interface corresponds to the 'g' element.
	/// </summary>
	public class SvgGElement : SvgTransformableElement , ISvgGElement, IEventTarget
	{
		#region Constructors

		internal SvgGElement(string prefix, string localname, string ns, SvgDocument doc) : base(prefix, localname, ns, doc) 
		{
			svgExternalResourcesRequired = new SvgExternalResourcesRequired(this);
			svgTests = new SvgTests(this);
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

		#region Events
		public bool DispatchEvent(IEvent e)
		{
			return ((Event)e).Propagate(this, true);
		}

		public void FireEvent(IEvent e)
		{
			DomEvent eventToFire = null;

			switch(e.Type)
			{
				case "mousemove":
					eventToFire = OnMouseMove;
					break;
				case "mouseup":
					eventToFire = OnMouseUp;
					break;
				case "mousedown":
					eventToFire = OnMouseDown;
					break;
				case "mouseover":
					eventToFire = OnMouseOver;
					break;
				case "mouseout":
					eventToFire = OnMouseOut;
					break;
				case "click":
					eventToFire = OnClick;
					break;
			}

			if(eventToFire != null) eventToFire(e);
		}

		public event DomEvent OnMouseMove;
		public event DomEvent OnMouseDown;
		public event DomEvent OnMouseUp;
		public event DomEvent OnMouseOut;
		public event DomEvent OnMouseOver;
		public event DomEvent OnClick;
		#endregion
	}
}
