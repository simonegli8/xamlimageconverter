using System;

namespace SharpVectors.Dom.Events
{
	public class EventException : Exception, IEventException
	{
		private EventExceptionCode _code;

		public EventException(EventExceptionCode code)
		{
			_code = code;
		}

		public EventExceptionCode Code
		{
			get{return _code;}
		}
	}
}
