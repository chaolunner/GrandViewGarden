using System;
using UniRx;

namespace UniECS
{
	public abstract class System : ISystem, IDisposableContainer, IDisposable
	{
		public IEventSystem EventSystem { get; set; }

		public IPoolManager PoolManager { get; set; }

		protected CompositeDisposable disposer = new CompositeDisposable ();

		public CompositeDisposable Disposer {
			get { return disposer; }
			set { disposer = value; }
		}

		public virtual void Dispose ()
		{
			Disposer.Dispose ();
		}
	}
}
