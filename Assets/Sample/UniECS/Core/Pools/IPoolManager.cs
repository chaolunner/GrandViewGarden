using System.Collections.Generic;

namespace UniECS
{
	public interface IPoolManager
	{
		IEnumerable<IPool> Pools { get; }

		IIdentityGenerator IdentityGenerator { get; }

		IPool CreatePool (string name);

		IPool GetPool (string name = null);

		void RemovePool (string name);
	}
}