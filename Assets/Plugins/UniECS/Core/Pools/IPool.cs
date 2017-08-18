using System.Collections.Generic;

namespace UniECS
{
	public interface IPool
	{
		string Name { get; }

		IEnumerable<IEntity> Entities { get; }

		IIdentityGenerator IdentityGenerator { get; }

		IEntity CreateEntity();
		void RemoveEntity (IEntity entity);
	}
}
