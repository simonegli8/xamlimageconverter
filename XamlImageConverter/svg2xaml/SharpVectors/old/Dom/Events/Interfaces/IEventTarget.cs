using System;

namespace SharpVectors.Dom.Events
{
	public interface IEventTarget
	{
		/*
		 * Use .NETs event capturing operators instead
		void AddEventListener(string type, IEventListener listener, bool useCapture);
		void RemoveEventListener(string type, IEventListener listener, bool useCapture);
		*/

		bool DispatchEvent(IEvent evt);

		void FireEvent(IEvent evt);

		event DomEvent OnMouseMove;
	}
}
