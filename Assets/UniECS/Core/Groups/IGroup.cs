using System.Collections.Generic;
using UniECS;
using System;
using UniRx;

namespace UniECS
{
	public interface IGroup
	{
		IEventSystem EventSystem { get; set; }

		IPool EntityPool { get; set; }

		string Name { get; set; }

		ReactiveCollection<IEntity> Entities { get; set; }

		IEnumerable<Type> Components { get; set; }

		Predicate<IEntity> Predicate { get; }
	}
}