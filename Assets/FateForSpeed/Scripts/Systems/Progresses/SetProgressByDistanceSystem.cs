using UniEasy.ECS;
using UnityEngine;
using UniEasy;
using UniRx;

[ContextMenuAttribute("Progresses/SetProgressByDistanceSystem")]
public class SetProgressByDistanceSystem : RuntimeSystem
{
    private IGroup progresses;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);

        progresses = this.Create(typeof(ViewComponent), typeof(SetProgressByDistance), typeof(ProgressComponent));
    }

    public override void OnEnable()
    {
        base.OnEnable();

        progresses.OnAdd().Subscribe(entity =>
        {
            var setProgressByDistance = entity.GetComponent<SetProgressByDistance>();
            var progressComponent = entity.GetComponent<ProgressComponent>();
            var viewComponent = entity.GetComponent<ViewComponent>();

            entity.OnListenerAsObservable().Subscribe(data =>
            {
                if (setProgressByDistance.Space == Space.Self)
                {
                    progressComponent.Progress.Value = setProgressByDistance.ProgressCurve.Evaluate(1 - Mathf.Clamp01(viewComponent.Transforms[0].InverseTransformVector(viewComponent.Transforms[0].position - data.Collider.transform.position).magnitude / setProgressByDistance.Distance));
                }
                else
                {
                    progressComponent.Progress.Value = setProgressByDistance.ProgressCurve.Evaluate(1 - Mathf.Clamp01(Vector3.Distance(viewComponent.Transforms[0].position, data.Collider.transform.position) / setProgressByDistance.Distance));
                }
            }).AddTo(this.Disposer).AddTo(setProgressByDistance.Disposer);
        }).AddTo(this.Disposer);
    }
}
