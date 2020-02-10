using UnityEngine.SceneManagement;
using UniRx.Triggers;
using UnityEngine;
using UniEasy.ECS;
using System.Linq;
using UniEasy;
using UniRx;

[ContextMenuAttribute("Loading/LoadSceneSystem")]
public class LoadSceneSystem : RuntimeSystem
{
    private IGroup sceneLoaders;
    private IGroup loadingScreens;
    private BoolReactiveProperty readyToLoad = new BoolReactiveProperty();
    private ReactiveCollection<SceneSetup> scenesToLoad = new ReactiveCollection<SceneSetup>();
    private ReactiveCollection<SceneSetup> scenesToUnload = new ReactiveCollection<SceneSetup>();

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);

        sceneLoaders = this.Create(typeof(SceneLoader));
        loadingScreens = this.Create(typeof(LoadingScreen), typeof(Animator));
    }

    public override void OnEnable()
    {
        base.OnEnable();

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;

        sceneLoaders.OnAdd().Subscribe(entity =>
        {
            var sceneLoader = entity.GetComponent<SceneLoader>();

            entity.OnListenerAsObservable<TriggerEnterEvent>().Select(_ => true)
            .Merge(entity.OnListenerAsObservable().Select(_ => true))
            .Subscribe(_ =>
            {
                foreach (var setup in sceneLoader.LoadQueue)
                {
                    if (setup.Operation == SceneLoader.Operation.Load)
                    {
                        EventSystem.Send(new LoadSceneEvent(setup.SceneSetup));
                    }
                    else if (setup.Operation == SceneLoader.Operation.Unload)
                    {
                        EventSystem.Send(new UnloadSceneEvent(setup.SceneSetup));
                    }
                }
            }).AddTo(this.Disposer).AddTo(sceneLoader.Disposer);
        }).AddTo(this.Disposer);

        loadingScreens.OnAdd().Subscribe(entity =>
        {
            var loadingScreen = entity.GetComponent<LoadingScreen>();

            loadingScreen.State.DistinctUntilChanged().Where(state => state == FadeState.FadedOut).Subscribe(_ =>
            {
                if (loadingScreens.Entities.Select(e => e.GetComponent<LoadingScreen>().State.Value).All(state => state != FadeState.FadingIn && state != FadeState.FadingOut))
                {
                    readyToLoad.Value = true;
                }
            }).AddTo(this.Disposer).AddTo(loadingScreen.Disposer);
        }).AddTo(this.Disposer);

        EventSystem.OnEvent<LoadSceneEvent>().Subscribe(evt =>
        {
            foreach (var setup in evt.LoadSceneSetup.Setups)
            {
                if ((setup.Mode == LoadSceneMode.Additive && SceneManager.GetSceneByPath(setup.Path).isLoaded) || scenesToLoad.Contains(setup))
                {
                    continue;
                }
                scenesToLoad.Add(setup);
            }
        }).AddTo(this.Disposer);

        EventSystem.OnEvent<UnloadSceneEvent>().Subscribe(evt =>
        {
            foreach (var setup in evt.UnloadSceneSetup.Setups)
            {
                var scene = SceneManager.GetSceneByPath(setup.Path);
                if (!scene.IsValid() || !scene.isLoaded || scenesToUnload.Contains(setup))
                {
                    continue;
                }
                scenesToUnload.Add(setup);
            }
        }).AddTo(this.Disposer);

        EventSystem.OnEvent<LoadSceneEvent>().Select(evt => evt.LoadSceneSetup).Merge(EventSystem.OnEvent<UnloadSceneEvent>().Select(evt => evt.UnloadSceneSetup))
        .Where(_ => !readyToLoad.Value && scenesToLoad.Count + scenesToUnload.Count > 0).Subscribe(setup =>
        {
            var isReady = true;
            foreach (var loadingScreen in loadingScreens.Entities.Select(entity => entity.GetComponent<LoadingScreen>()))
            {
                if (setup.LoadingMask.Contains(loadingScreen.Layer))
                {
                    loadingScreen.State.Value = FadeState.FadingOut;
                    isReady = false;
                }
            }
            readyToLoad.Value = isReady;
        }).AddTo(this.Disposer);

        scenesToLoad.ObserveAdd().Subscribe(evt =>
        {
            readyToLoad.StartWith(readyToLoad.Value).Where(isReady => isReady).FirstOrDefault().Subscribe(_ =>
            {
                SceneManager.LoadSceneAsync(evt.Value.Path, evt.Value.Mode);
            }).AddTo(this.Disposer);
        }).AddTo(this.Disposer);

        scenesToUnload.ObserveAdd().Subscribe(evt =>
        {
            readyToLoad.StartWith(readyToLoad.Value).Where(isReady => isReady).FirstOrDefault().Subscribe(_ =>
            {
                SceneManager.UnloadSceneAsync(evt.Value.Path);
            }).AddTo(this.Disposer);
        }).AddTo(this.Disposer);

        scenesToLoad.ObserveRemove().Merge(scenesToUnload.ObserveRemove())
        .Where(_ => scenesToLoad.Count <= 0 && scenesToUnload.Count <= 0)
        .Subscribe(_ =>
        {
            readyToLoad.Value = false;

            Resources.UnloadUnusedAssets();

            foreach (var loadingScreen in loadingScreens.Entities.Select(entity => entity.GetComponent<LoadingScreen>()).Where(loadingScreen => loadingScreen.State.Value == FadeState.FadingOut || loadingScreen.State.Value == FadeState.FadedOut))
            {
                loadingScreen.State.Value = FadeState.FadingIn;
            }
        }).AddTo(this.Disposer);
    }

    public override void OnDisable()
    {
        base.OnDisable();

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        foreach (var setup in scenesToLoad)
        {
            if (setup.Path == scene.path)
            {
                scenesToLoad.Remove(setup);
                break;
            }
        }
    }

    private void OnSceneUnloaded(Scene scene)
    {
        foreach (var setup in scenesToUnload)
        {
            if (setup.Path == scene.path)
            {
                scenesToUnload.Remove(setup);
                break;
            }
        }
    }
}
