using System;
using SharpVectors.Dom.Views;

namespace SharpVectors.Dom.Events
{
	public interface IUIEvent : IEvent
	{
		IAbstractView View{get;}
		double Detail{get;}
		void InitUIEvent(string typeArg, bool canBubbleArg, bool cancelableArg, 
			IAbstractView viewArg, double detailArg);
	}
}
