using UniRx;

namespace UniECS
{
	public interface IEventSystem
	{
		void Publish<T> (T message);

		IObservable<T> OnEvent<T> ();
	}
}