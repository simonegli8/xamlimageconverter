using System;
using SharpVectors.Dom.Views;

namespace SharpVectors.Dom.Events
{
	public interface IMouseEvent : IUIEvent 
	{
        double ScreenX { get;}
        double ScreenY { get;}
        double ClientX { get;}
        double ClientY { get;}
		bool CtrlKey{get;}
		bool ShiftKey{get;}
		bool AltKey{get;}
		bool MetaKey{get;}
		ushort Button{get;}
		IEventTarget RelatedTarget{get;}
		void InitMouseEvent(string typeArg, bool canBubbleArg, bool cancelableArg, IAbstractView viewArg, 
			double detailArg, double screenXArg, double screenYArg, double clientXArg, double clientYArg, bool ctrlKeyArg, 
			bool altKeyArg, bool shiftKeyArg, bool metaKeyArg, ushort buttonArg, IEventTarget relatedTargetArg);
	}
}
