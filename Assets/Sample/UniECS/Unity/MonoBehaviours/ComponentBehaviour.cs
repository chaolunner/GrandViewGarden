using UnityEngine;
using System;
using UniRx;

namespace UniECS
{
	public abstract class ComponentBehaviour : MonoBehaviour, IDisposable
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

		private CompositeDisposable disposer = new CompositeDisposable ();

		public CompositeDisposable Disposer {
			get { return disposer; }
			set { disposer = value; }
		}

		protected bool IsQuitting = false;

		public virtual void OnDestroy ()
		{
			Dispose ();

			if (IsQuitting) {
				return;
			}

			if (EventSystem == null) {
				Debug.LogWarning ("A COMPONENT ON " + this.gameObject.name + " WAS NOT INJECTED PROPERLY!");
			}
			EventSystem.Publish (new ComponentDestroyed () { Component = this });
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

		public virtual void OnApplicationQuit ()
		{
			IsQuitting = true;
		}
	}
}
