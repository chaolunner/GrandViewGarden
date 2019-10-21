using UniRx.Triggers;
using UnityEngine;
using UniEasy.ECS;
using UniRx;

public class LevelSelectionSystem : SystemBehaviour
{
    private IGroup levelSelecters;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);

        levelSelecters = this.Create(typeof(LevelSelecter), typeof(ViewComponent));
    }

    public override void OnEnable()
    {
        base.OnEnable();

        levelSelecters.OnAdd().Subscribe(entity =>
        {
            var viewComponent = entity.GetComponent<ViewComponent>();
            var levelSelecter = entity.GetComponent<LevelSelecter>();

            if (levelSelecter.Mode == LevelSelectionMode.Smart)
            {
                levelSelecter.Index = viewComponent.Transforms[0].GetSiblingIndex();
            }

            entity.OnListenerAsObservable().Subscribe(_ =>
            {
                if (levelSelecter.Mode == LevelSelectionMode.Single || levelSelecter.Mode == LevelSelectionMode.Smart)
                {
                    PlayerPrefs.SetInt(PlayerPrefsParameters.CurrentLevelIndex, levelSelecter.Index);
                    EventSystem.Publish(new LevelSelectedEvent(levelSelecter.Index));
                }
                else if (levelSelecter.Mode == LevelSelectionMode.Additive)
                {
                    var index = PlayerPrefs.GetInt(PlayerPrefsParameters.CurrentLevelIndex) + levelSelecter.Index;
                    PlayerPrefs.SetInt(PlayerPrefsParameters.CurrentLevelIndex, index);
                    EventSystem.Publish(new LevelSelectedEvent(index));
                }
            }).AddTo(this.Disposer).AddTo(levelSelecter.Disposer);
        }).AddTo(this.Disposer);
    }
}
