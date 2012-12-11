using System;
using System.Xml;

using SharpVectors.Dom.Svg;


namespace SharpVectors.Dom.Events
{
	public class Event : IEvent
	{
		protected Event()
		{
		}

		public Event(string eventTypeArg, bool canBubbleArg, bool cancelableArg)
		{
			this.InitEvent(eventTypeArg, canBubbleArg, cancelableArg);
		}

		public bool Propagate(IEventTarget target, bool useCapture)
		{
			_target = target;
			XmlElement elm = Target as XmlElement;
			XmlNodeList ancestors = elm.SelectNodes("ancestor::*");
			if(useCapture && !stopped)
			{
				_eventPhase = EventPhase.CapturingPhase;
				for(int i = 0; i<ancestors.Count; i++)
				{
					if(stopped) break;
					IEventTarget ancestor = ancestors[i] as IEventTarget;
					if(ancestor != null)
					{
						_currentTarget = ancestor;
						ancestor.FireEvent(this);
					}
				}
			}
			
			if(!stopped)
			{
				_eventPhase = EventPhase.AtTarget;
				_currentTarget = Target;
				Target.FireEvent(this);
			}

			if(Bubbles && !stopped)
			{
				_eventPhase = EventPhase.BubblingPhase;
				for(int i = ancestors.Count; i>=0; i--)
				{
					if(stopped) break;
					IEventTarget ancestor = ancestors[i] as IEventTarget;
					if(ancestor != null)
					{
						_currentTarget = ancestor;
						ancestor.FireEvent(this);
					}
				} 
			}
			return stopped;
		}

		#region Implementation of IEvent
		private bool stopped = false;
		public void StopPropagation()
		{
			stopped = true;
		}

		public void PreventDefault()
		{
			throw new NotImplementedException();		
		}

		public void InitEvent(string eventTypeArg, bool canBubbleArg, bool cancelableArg)
		{
			if(eventTypeArg == null || eventTypeArg.Length == 0)
			{
				throw new EventException(EventExceptionCode.UnspecifiedEventTypeErr);
			}
			else
			{
				_type = eventTypeArg;
				_timeStamp = DateTime.Now;
				_cancelable = cancelableArg;
				_bubbles = canBubbleArg;
			}
		}

		protected string _type;
		public string Type
		{
			get
			{
				return _type;
			}
		}

		private IEventTarget _target;
		public IEventTarget Target
		{
			get
			{
				return _target;
			}
		}

		private IEventTarget _currentTarget;
		public IEventTarget CurrentTarget
		{
			get
			{
				return _currentTarget;
			}
		}

		public EventPhase _eventPhase;
		public EventPhase EventPhase
		{
			get
			{
                return _eventPhase;			
			}
		}

		private bool _bubbles;
		public bool Bubbles
		{
			get
			{
				return _bubbles;
			}
		}

		private bool _cancelable;
		public bool Cancelable
		{
			get
			{
				return _cancelable;
			}
		}

		private DateTime _timeStamp;
		public DateTime TimeStamp
		{
			get
			{
				return _timeStamp;
			}
		}
		#endregion
	}
}
