using UniEasy.ECS;
using UnityEngine;
using UniEasy;
using UniRx;

[ContextMenuAttribute("Loading/LoadingScreenSystem")]
public class LoadingScreenSystem : RuntimeSystem
{
    private IGroup loadingScreens;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);

        loadingScreens = this.Create(typeof(LoadingScreen), typeof(Animator), typeof(EventsListener));
    }

    public override void OnEnable()
    {
        base.OnEnable();

        loadingScreens.OnAdd().Subscribe(entity =>
        {
            var loadingScreen = entity.GetComponent<LoadingScreen>();
            var listener = entity.GetComponent<EventsListener>();
            var animator = entity.GetComponent<Animator>();

            loadingScreen.State.DistinctUntilChanged().Subscribe(state =>
            {
                if (state == FadeState.FadingIn)
                {
                    animator.Play(AnimatorStates.FadingIn, 0, 0);
                }
                else if (state == FadeState.FadingOut)
                {
                    animator.Play(AnimatorStates.FadingOut, 0, 0);
                }
            }).AddTo(this.Disposer).AddTo(loadingScreen.Disposer);

            listener.OnEvent<FSMFadedInEvent>().Where(evt => loadingScreen.State.Value != FadeState.FadedIn).Subscribe(_ =>
            {
                loadingScreen.State.Value = FadeState.FadedIn;
            }).AddTo(this.Disposer).AddTo(loadingScreen.Disposer);

            listener.OnEvent<FSMFadedOutEvent>().Where(evt => loadingScreen.State.Value != FadeState.FadedOut).Subscribe(_ =>
            {
                loadingScreen.State.Value = FadeState.FadedOut;
            }).AddTo(this.Disposer).AddTo(loadingScreen.Disposer);
        }).AddTo(this.Disposer);
    }
}
