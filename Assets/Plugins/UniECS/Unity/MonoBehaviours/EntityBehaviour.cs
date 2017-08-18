using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace UniECS
{
	public class EntityBehaviour : ComponentBehaviour
	{
		public IPool Pool {
			get {
				if (pool != null) {
					return pool;
				} else if (string.IsNullOrEmpty (PoolName)) {
					return (pool = PoolManager.GetPool ());
				} else if (PoolManager.Pools.All (x => x.Name != PoolName)) {
					return (pool = PoolManager.CreatePool (PoolName));
				} else {
					return (pool = PoolManager.GetPool (PoolName));
				}
			}
			set { pool = value; }
		}

		private IPool pool;

		public IEntity Entity {
			get {
				return entity == null ? (entity = Pool.CreateEntity ()) : entity;
			}
			set {
				entity = value;
			}
		}

		private IEntity entity;

		[SerializeField]
		[HideInInspector]
		public string PoolName;

		[SerializeField]
		[HideInInspector]
		public bool RemoveEntityOnDestroy = true;

		public override void Awake ()
		{
			base.Awake ();

			var monoBehaviours = GetComponents<Component> ();
			foreach (var mb in monoBehaviours) {
				if (mb == null) {
					Debug.LogWarning ("Component on " + this.gameObject.name + " is null!");
				} else {
					if (mb.GetType () != typeof(Transform)) {
						if (mb.GetType () == typeof(EntityBehaviour)) {
							if (!Entity.HasComponent<EntityBehaviour> ())
								Entity.AddComponent (mb);
						} else {
							Entity.AddComponent (mb);
						}
					}
				}
			}
		}

		public override void OnDestroy ()
		{
			if (IsQuitting) {
				return;
			}
			if (!RemoveEntityOnDestroy) {
				return;
			}
			IPool poolToUse;

			if (string.IsNullOrEmpty (PoolName)) {
				poolToUse = PoolManager.GetPool ();
			} else if (PoolManager.Pools.All (x => x.Name != PoolName)) {
				poolToUse = PoolManager.CreatePool (PoolName);
			} else {
				poolToUse = PoolManager.GetPool (PoolName);
			}

			poolToUse.RemoveEntity (Entity);

			base.OnDestroy ();
		}
	}
}