using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UniRx;

namespace UniECS
{
	public class Group : IGroup, IDisposableContainer, IDisposable
	{
		public IEventSystem EventSystem { get; set; }

		public IPool EntityPool { get; set; }

		public string Name { get; set; }

		ReactiveCollection<IEntity> entities = new ReactiveCollection<IEntity> ();

		public ReactiveCollection<IEntity> Entities {
			get { return entities; }
			set { entities = value; }
		}

		public IEnumerable<Type> Components { get; set; }

		public Predicate<IEntity> Predicate { get; set; }

		protected CompositeDisposable disposer = new CompositeDisposable ();

		public CompositeDisposable Disposer {
			get { return disposer; }
			set { disposer = value; }
		}

		public Group (params Type[] components)
		{
			Components = components;
			Predicate = null;
		}

		public void Setup (IEventSystem eventSystem, IPoolManager poolManager)
		{
			EventSystem = eventSystem;
			EntityPool = poolManager.GetPool ();

			foreach (IEntity entity in EntityPool.Entities) {
				if (entity.HasComponents (Components.ToArray ())) {
					AddEntity (entity);
				}
			}

			EventSystem.OnEvent<EntityAddedEvent> ().Where (e => e.Entity.HasComponents (Components.ToArray ())).Subscribe (e => {
				AddEntity (e.Entity);
			}).AddTo (this);

			EventSystem.OnEvent<EntityRemovedEvent> ().Where (e => Entities.Contains (e.Entity)).Subscribe (e => {
				RemoveEntity (e.Entity);
			}).AddTo (this);

			EventSystem.OnEvent<ComponentAddedEvent> ().Where (e => e.Entity.HasComponents (Components.ToArray ()) && Entities.Contains (e.Entity) == false).Subscribe (e => {
				AddEntity (e.Entity);
			}).AddTo (this);

			EventSystem.OnEvent<ComponentRemovedEvent> ().Where (e => Components.Contains (e.Component.GetType ()) && Entities.Contains (e.Entity)).Subscribe (e => {
				RemoveEntity (e.Entity);
			}).AddTo (this);
		}

		void AddEntity (IEntity entity)
		{
			Entities.Add (entity);
		}

		void RemoveEntity (IEntity entity)
		{
			Entities.Remove (entity);
		}

		public void Dispose ()
		{
			Disposer.Dispose ();
		}
	}
}