using UniRx.Triggers;
using UniEasy.ECS;
using UnityEngine;
using UniEasy;
using System;
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

            if (entity.HasComponent<TriggerEnterListener>() || entity.HasComponent<TriggerStayListener>() || entity.HasComponent<TriggerExitListener>())
            {
                IObservable<Collider> enterObservable = null;
                IObservable<Collider> stayObservable = null;
                IObservable<Collider> exitObservable = null;

                if (entity.HasComponent<TriggerEnterListener>())
                {
                    enterObservable = entity.GetComponent<TriggerEnterListener>().Targets.OnTriggerEnterAsObservable();
                }
                if (entity.HasComponent<TriggerStayListener>())
                {
                    stayObservable = entity.GetComponent<TriggerStayListener>().Targets.OnTriggerStayAsObservable();
                }
                if (entity.HasComponent<TriggerExitListener>())
                {
                    exitObservable = entity.GetComponent<TriggerExitListener>().Targets.OnTriggerExitAsObservable();
                }

                Observable.SafeMerge(enterObservable, stayObservable, exitObservable).Subscribe(col =>
                {
                    if (setProgressByDistance.Space == Space.Self)
                    {
                        progressComponent.Progress.Value = setProgressByDistance.ProgressCurve.Evaluate(1 - Mathf.Clamp01(viewComponent.Transforms[0].InverseTransformVector(viewComponent.Transforms[0].position - col.transform.position).magnitude / setProgressByDistance.Distance));
                    }
                    else
                    {
                        progressComponent.Progress.Value = setProgressByDistance.ProgressCurve.Evaluate(1 - Mathf.Clamp01(Vector3.Distance(viewComponent.Transforms[0].position, col.transform.position) / setProgressByDistance.Distance));
                    }
                }).AddTo(this.Disposer).AddTo(progressComponent.Disposer);
            }
        }).AddTo(this.Disposer);
    }
}
