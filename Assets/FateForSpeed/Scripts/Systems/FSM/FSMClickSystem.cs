using UniRx.Triggers;
using UnityEngine;
using UniEasy.ECS;
using System.Linq;
using UniEasy;
using UniRx;

[ContextMenuAttribute("FSM/ClickSystem")]
public class FSMClickSystem : RuntimeSystem
{
    private IGroup clickListeners;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);

        clickListeners = this.Create(typeof(Animator), typeof(ClickListener));
    }

    public override void OnEnable()
    {
        base.OnEnable();

        clickListeners.OnAdd().Subscribe(entity =>
        {
            var listener = entity.GetComponent<ClickListener>();
            var animator = entity.GetComponent<Animator>();

            listener.Targets.OnPointerClickAsObservable()
            .Where(_ => animator.IsValid(AnimatorParameters.IsClick))
            .Subscribe(_ =>
            {
                animator.SetBool(AnimatorParameters.IsClick, true);
            }).AddTo(this.Disposer).AddTo(listener.Disposer);
        }).AddTo(this.Disposer);
    }
}
