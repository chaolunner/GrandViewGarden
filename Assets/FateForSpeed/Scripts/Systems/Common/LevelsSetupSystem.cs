using UnityEngine;
using UniEasy.ECS;
using UniRx;

public class LevelsSetupSystem : SystemBehaviour
{
    private IGroup levelSetupers;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);

        levelSetupers = this.Create(typeof(LevelSetuper), typeof(ViewComponent));
    }

    public override void OnEnable()
    {
        base.OnEnable();

        levelSetupers.OnAdd().Subscribe(entity =>
        {
            var viewComponent = entity.GetComponent<ViewComponent>();
            var levelSetuper = entity.GetComponent<LevelSetuper>();

            for (int i = 0; i < viewComponent.Transforms[0].childCount; i++)
            {
                levelSetuper.Levels.Add(viewComponent.Transforms[0].GetChild(i).GetComponent<FSMToggle>());
            }

            levelSetuper.Index.Value = PlayerPrefs.GetInt(PlayerPrefsParameters.CurrentLevelIndex);

            levelSetuper.Index.DistinctUntilChanged().Where(i => i < levelSetuper.Levels.Count).Select(i => levelSetuper.Levels[i]).Subscribe(toggle =>
            {
                toggle.IsOn.Value = true;
            }).AddTo(this.Disposer).AddTo(levelSetuper.Disposer);

            EventSystem.OnEvent<LevelSelectedEvent>().Select(evt => evt.Index).Subscribe(index =>
            {
                levelSetuper.Index.Value = index;
            }).AddTo(this.Disposer).AddTo(levelSetuper.Disposer);
        }).AddTo(this.Disposer);
    }
}
