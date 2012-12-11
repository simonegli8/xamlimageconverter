using System;

namespace SharpVectors.Dom.Events
{
	public enum EventPhase{
		CapturingPhase = 1,
		AtTarget = 2,
		BubblingPhase = 3
	}

	public interface IEvent 
	{
		string Type{get;}
		IEventTarget Target{get;}
		IEventTarget CurrentTarget{get;}
		EventPhase EventPhase{get;}
		bool Bubbles{get;}
		bool Cancelable{get;}
		DateTime TimeStamp{get;}
			
		void StopPropagation();
		void PreventDefault();
		void InitEvent(string eventTypeArg, bool canBubbleArg, bool cancelableArg);
	}
}
