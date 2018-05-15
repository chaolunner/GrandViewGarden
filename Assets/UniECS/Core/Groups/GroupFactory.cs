using System;

namespace UniECS
{
	public class GroupFactory
	{
		static private GroupFactory instance;

		static public GroupFactory Instance {
			get {
				if (instance == null) {
					instance = new GroupFactory ();
				}
				return instance;
			}
		}

		public Group Create (Type[] types)
		{
			var group = new Group (types);
			group.Setup (EventSystem.Instance, PoolManager.Instance);
			return group;
		}
	}
}