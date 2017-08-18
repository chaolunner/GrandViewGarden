using UniRx;

namespace UniECS
{
	public static class IGroupExtensions
	{
		public static IObservable<IEntity> OnAdd (this IGroup group)
		{
			return group.Entities.ObserveAdd ().Select (x => x.Value).StartWith (group.Entities);
		}

		public static IObservable<IEntity> OnRemove (this IGroup group)
		{
			return group.Entities.ObserveRemove ().Select (x => x.Value);
		}
	}
}