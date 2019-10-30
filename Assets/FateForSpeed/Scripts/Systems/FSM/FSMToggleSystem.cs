using UnityEngine;
using UniEasy.ECS;
using UniEasy;
using UniRx;

[ContextMenuAttribute("FSM/ToggleSystem")]
public class FSMToggleSystem : RuntimeSystem
{
    private IGroup toggles;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);

        toggles = this.Create(typeof(Animator), typeof(FSMToggle));
    }

    public override void OnEnable()
    {
        base.OnEnable();

        toggles.OnAdd().Subscribe(entity =>
        {
            var toggle = entity.GetComponent<FSMToggle>();
            var animator = entity.GetComponent<Animator>();

            if (entity.HasComponent<SerializableEventListener>())
            {
                var listener = entity.GetComponent<SerializableEventListener>();

                listener.OnEvent<FSMButtonClickedEvent>().Subscribe(_ =>
                {
                    toggle.IsOn.Value = !toggle.IsOn.Value;
                }).AddTo(this.Disposer).AddTo(toggle.Disposer);

                listener.OnEvent<TriggerEnterEvent>(true).Subscribe(_ =>
                {
                    toggle.IsOn.Value = true;
                }).AddTo(this.Disposer).AddTo(toggle.Disposer);

                listener.OnEvent<TriggerExitEvent>(true).Subscribe(_ =>
                {
                    toggle.IsOn.Value = false;
                }).AddTo(this.Disposer).AddTo(toggle.Disposer);
            }

            var onChanged = toggle.IsOn.DistinctUntilChanged().TakeWhile(_ => toggle.Group == null);

            onChanged.Subscribe(isOn =>
            {
                animator.SetBool(AnimatorParameters.IsOn, isOn);
            }).AddTo(this.Disposer).AddTo(toggle.Disposer);

            onChanged.FirstOrDefault().Where(isOn => isOn).Subscribe(_ =>
            {
                animator.Play(AnimatorStates.Opened, 0, 0);
            }).AddTo(this.Disposer).AddTo(toggle.Disposer);
        }).AddTo(this.Disposer);
    }
}
