using UnityEngine;
using UniEasy.ECS;
using UniEasy;
using UniRx;

[ContextMenuAttribute("FSM/ProgressSystem")]
public class FSMProgressSystem : RuntimeSystem
{
    private IGroup progresses;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);

        progresses = this.Create(typeof(Animator), typeof(ProgressComponent));
    }

    public override void OnEnable()
    {
        base.OnEnable();

        progresses.OnAdd().Subscribe(entity =>
        {
            var progressComponent = entity.GetComponent<ProgressComponent>();
            var animator = entity.GetComponent<Animator>();

            progressComponent.Progress.DistinctUntilChanged()
            .Where(_ => animator.IsValid(AnimatorParameters.Progress))
            .Subscribe(progress =>
            {
                animator.SetFloat(AnimatorParameters.Progress, progress);
            }).AddTo(this.Disposer).AddTo(progressComponent.Disposer);
        }).AddTo(this.Disposer);
    }
}
