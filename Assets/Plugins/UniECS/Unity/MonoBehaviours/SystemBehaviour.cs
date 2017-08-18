using UnityEngine;
using System;
using UniRx;

namespace UniECS
{
	public abstract class SystemBehaviour : MonoBehaviour, ISystem, IDisposableContainer, IDisposable
	{
		public IEventSystem EventSystem {
			get {
				if (eventSystem == null) {
					eventSystem = UniECS.EventSystem.Instance;
				}
				return eventSystem;
			}
			set { eventSystem = value; }
		}

		private IEventSystem eventSystem;

		public IPoolManager PoolManager {
			get {  
				if (poolManager == null) {
					poolManager = UniECS.PoolManager.Instance;
				}
				return poolManager; 
			}
			set { poolManager = value; }
		}

		private IPoolManager poolManager;

		protected GroupFactory GroupFactory { 
			get {  
				if (groupFactory == null) {
					groupFactory = UniECS.GroupFactory.Instance;
				}
				return groupFactory; 
			}
			set { groupFactory = value; }
		}

		private GroupFactory groupFactory;

		private CompositeDisposable disposer = new CompositeDisposable ();

		public CompositeDisposable Disposer {
			get { return disposer; }
			set { disposer = value; }
		}

		void OnDestroy ()
		{
			Dispose ();
		}

		public virtual void Awake ()
		{
		}

		public virtual void Start ()
		{
		}

		public virtual void Dispose ()
		{
			Disposer.Dispose ();
		}
	}
}
