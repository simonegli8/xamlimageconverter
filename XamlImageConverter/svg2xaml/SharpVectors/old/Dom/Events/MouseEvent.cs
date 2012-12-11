using System;


using SharpVectors.Dom.Views;
using System.Windows.Input;

namespace SharpVectors.Dom.Events
{
	public class MouseEvent : UIEvent, IMouseEvent
	{
		public MouseEvent(string type, MouseButtonEventArgs e)
		{
			InitEvent(type, true, false);
			this._detail = e.ClickCount;
			this._screenX = e.GetPosition(null).X;
            this._screenY = e.GetPosition(null).Y;
			

		}

		public MouseEvent(string typeArg, bool canBubbleArg, bool cancelableArg, IAbstractView viewArg, double detailArg, double screenXArg, double screenYArg, double clientXArg, double clientYArg, bool ctrlKeyArg, bool altKeyArg, bool shiftKeyArg, bool metaKeyArg, ushort buttonArg, SharpVectors.Dom.Events.IEventTarget relatedTargetArg)
		{
			InitMouseEvent(typeArg, canBubbleArg, cancelableArg, viewArg, detailArg, screenXArg, screenYArg, clientXArg, clientYArg, ctrlKeyArg, altKeyArg, shiftKeyArg, metaKeyArg, buttonArg, relatedTargetArg);
		}

		#region Implementation of IMouseEvent
		public void InitMouseEvent(string typeArg, bool canBubbleArg, bool cancelableArg, IAbstractView viewArg, double detailArg, double screenXArg, double screenYArg, double clientXArg, double clientYArg, bool ctrlKeyArg, bool altKeyArg, bool shiftKeyArg, bool metaKeyArg, ushort buttonArg, SharpVectors.Dom.Events.IEventTarget relatedTargetArg)
		{
			base.InitUIEvent(typeArg, canBubbleArg, cancelableArg, viewArg, detailArg);

			_screenX = screenXArg;
			_screenY = screenYArg; 
			_clientX = clientXArg; 
			_clientY = clientYArg; 
			_ctrlKey = ctrlKeyArg; 
			_altKey = altKeyArg; 
			_shiftKey = shiftKeyArg;
			_metaKey = metaKeyArg;
			_button = buttonArg; 
			_relatedTarget = relatedTargetArg;
		}

		private double _screenX;
        public double ScreenX
		{
			get
			{
				return 0;
			}
		}

        private double _screenY;
        public double ScreenY
		{
			get
			{
				return 0;
			}
		}

        private double _clientX;
        public double ClientX
		{
			get
			{
				return 0;
			}
		}

        private double _clientY;
        public double ClientY
		{
			get
			{
				return 0;
			}
		}

		private bool _ctrlKey;
		public bool CtrlKey
		{
			get
			{
				return true;
			}
		}

		private bool _shiftKey;
		public bool ShiftKey
		{
			get
			{
				return true;
			}
		}

		private bool _altKey;
		public bool AltKey
		{
			get
			{
				return true;
			}
		}

		private bool _metaKey;
		public bool MetaKey
		{
			get
			{
				return true;
			}
		}

		private ushort _button;
		public ushort Button
		{
			get
			{
				return 0;
			}
		}

		private IEventTarget _relatedTarget;
		public IEventTarget RelatedTarget
		{
			get
			{
				return null;
			}
		}
		#endregion
	}
}
