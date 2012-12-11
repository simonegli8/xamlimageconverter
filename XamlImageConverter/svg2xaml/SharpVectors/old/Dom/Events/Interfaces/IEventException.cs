using System;

namespace SharpVectors.Dom.Events
{
	public enum EventExceptionCode{UnspecifiedEventTypeErr}

	public interface IEventException
	{
		EventExceptionCode Code{get;}
	}
}
