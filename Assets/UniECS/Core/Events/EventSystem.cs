using UniRx;

namespace UniECS
{
	public class EventSystem : IEventSystem
	{
		static private EventSystem instance;

		static public EventSystem Instance {
			get {
				if (instance == null) {
					instance = new EventSystem (UniRx.MessageBroker.Default);
				}
				return instance;
			}
		}

		public IMessageBroker MessageBroker { get; private set; }

		public EventSystem (IMessageBroker messageBroker)
		{
			MessageBroker = messageBroker;
		}

		public void Publish<T> (T message)
		{
			MessageBroker.Publish (message);
		}

		public IObservable<T> OnEvent<T> ()
		{
			return MessageBroker.Receive<T> ();
		}
	}
}