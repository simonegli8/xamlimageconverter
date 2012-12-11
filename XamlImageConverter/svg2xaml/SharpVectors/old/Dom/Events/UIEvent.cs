using System;
using SharpVectors.Dom.Views;

namespace SharpVectors.Dom.Events
{
	public class UIEvent : Event, IUIEvent
	{
		protected UIEvent()
		{
		}

		public UIEvent(string typeArg, bool canBubbleArg, bool cancelableArg, SharpVectors.Dom.Views.IAbstractView viewArg, double detailArg) :
			base()
		{
			InitUIEvent(typeArg, canBubbleArg, cancelableArg, viewArg, detailArg);
		}

		#region Implementation of IUIEvent
		public void InitUIEvent(string typeArg, bool canBubbleArg, bool cancelableArg, SharpVectors.Dom.Views.IAbstractView viewArg, double detailArg)
		{
			base.InitEvent(typeArg, canBubbleArg, cancelableArg);
		
			_view = viewArg;
			_detail = detailArg;
		}

		private IAbstractView _view;
		public IAbstractView View
		{
			get
			{
				return _view;
			}
		}

		protected double _detail;
		public double Detail
		{
			get
			{
				return _detail;
			}
		}
		#endregion
	}
}
