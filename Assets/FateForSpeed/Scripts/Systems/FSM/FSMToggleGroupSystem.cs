using UnityEngine;
using UniEasy.ECS;
using System.Linq;
using UniEasy;
using UniRx;

[ContextMenuAttribute("FSM/ToggleGroupSystem")]
public class FSMToggleGroupSystem : RuntimeSystem
{
    private IGroup toggleGroups;
    private IGroup toggles;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);

        toggleGroups = this.Create(typeof(ViewComponent), typeof(FSMToggleGroup));
        toggles = this.Create(typeof(ViewComponent), typeof(FSMToggle), typeof(Animator));
    }

    public override void OnEnable()
    {
        base.OnEnable();

        toggleGroups.OnAdd().Subscribe(groupEntity =>
        {
            var group = groupEntity.GetComponent<FSMToggleGroup>();
            var openToggleDisposer = new CompositeDisposable();

            group.Toggles.OnAdd().Subscribe(toggleEntity =>
            {
                var animator = toggleEntity.GetComponent<Animator>();
                var toggle = toggleEntity.GetComponent<FSMToggle>();

                toggle.Group = group;

                toggle.IsOn.DistinctUntilChanged().FirstOrDefault().Where(isOn => isOn).Subscribe(_ =>
                {
                    foreach (var entity in group.Toggles)
                    {
                        if (toggleEntity != entity)
                        {
                            entity.GetComponent<Animator>().Play(AnimatorStates.Closed, 0, 0);
                            entity.GetComponent<FSMToggle>().IsOn.Value = false;
                        }
                    }
                    animator.Play(AnimatorStates.Opened, 0, 0);
                }).AddTo(this.Disposer).AddTo(group.Disposer).AddTo(toggle.Disposer);

                toggle.IsOn.DistinctUntilChanged().Subscribe(isOn =>
                {
                    if (isOn)
                    {
                        openToggleDisposer.Clear();

                        foreach (var entity in group.Toggles)
                        {
                            if (toggleEntity != entity)
                            {
                                entity.GetComponent<FSMToggle>().IsOn.Value = false;
                            }
                        }

                        if (!IsCloseCompleted(toggleEntity))
                        {
                            EventSystem.OnEvent<FSMToggleClosedEvent>()
                            .Where(evt => group.Toggles.Any(entity => entity.GetComponent<Animator>() == evt.Animator) && IsCloseCompleted(toggleEntity))
                            .Subscribe(evt =>
                            {
                                animator.SetBool(AnimatorParameters.IsOn, true);
                            }).AddTo(this.Disposer).AddTo(group.Disposer).AddTo(toggle.Disposer).AddTo(openToggleDisposer);
                        }
                        else
                        {
                            animator.SetBool(AnimatorParameters.IsOn, true);
                        }
                    }
                    else
                    {
                        animator.SetBool(AnimatorParameters.IsOn, false);
                    }
                }).AddTo(this.Disposer).AddTo(group.Disposer).AddTo(toggle.Disposer);
            }).AddTo(this.Disposer).AddTo(group.Disposer);
        }).AddTo(this.Disposer);

        toggles.OnAdd().Subscribe(toggleEntity =>
        {
            var toggleView = toggleEntity.GetComponent<ViewComponent>();

            toggleGroups.OnAdd().Subscribe(groupEntity =>
            {
                var groupView = groupEntity.GetComponent<ViewComponent>();
                var group = groupEntity.GetComponent<FSMToggleGroup>();

                if (toggleView.Transforms[0].parent == groupView.Transforms[0])
                {
                    group.Toggles.Add(toggleEntity);
                }
            }).AddTo(this.Disposer).AddTo(toggleView.Disposer);
        }).AddTo(this.Disposer);
    }

    private bool IsCloseCompleted(IEntity toggleEntity)
    {
        var toggle = toggleEntity.GetComponent<FSMToggle>();
        foreach (var entity in toggle.Group.Toggles)
        {
            if (toggleEntity != entity && entity.GetComponent<Animator>().State(0) != AnimatorStates.Closed)
            {
                return false;
            }
        }
        return true;
    }
}
