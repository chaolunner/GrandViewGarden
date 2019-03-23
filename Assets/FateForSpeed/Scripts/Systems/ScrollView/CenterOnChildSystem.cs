using System.Collections.Generic;
using UniRx.Triggers;
using UniEasy.ECS;
using UnityEngine;
using DG.Tweening;
using UniEasy;
using UniRx;

[ContextMenuAttribute("ScrollView/CenterOnChildSystem")]
public class CenterOnChildSystem : RuntimeSystem
{
    private IGroup centerOnChilds;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);

        centerOnChilds = this.Create(typeof(ViewComponent), typeof(CenterOnChild));
    }

    public override void OnEnable()
    {
        base.OnEnable();

        centerOnChilds.OnAdd().Subscribe(entity =>
        {
            var centerOnChild = entity.GetComponent<CenterOnChild>();
            var viewComponent = entity.GetComponent<ViewComponent>();
            var childsChangeDisposer = new CompositeDisposable();

            Tweener centeringTweener = null;

            centerOnChild.Index.Where(_ => centerOnChild.Content && centerOnChild.Content.childCount > 0).Subscribe(i =>
            {
                var index = centerOnChild.Content.childCount - 2 * centerOnChild.Space >= 0 ? Mathf.Clamp(i, centerOnChild.Space, centerOnChild.Content.childCount - centerOnChild.Space - 1) : Mathf.CeilToInt(0.5f * centerOnChild.Content.childCount);

                centerOnChild.Index.Value = index;
                if (centeringTweener != null)
                {
                    centeringTweener.Kill();
                    centeringTweener = null;
                }

                var target = centerOnChild.Content.GetChild(index);
                var origin = centerOnChild.Content.localPosition;
                var distance = viewComponent.Transforms[0].position - target.position;
                var duration = distance.magnitude / centerOnChild.Speed;

                distance = centerOnChild.Content.InverseTransformVector(distance);
                centeringTweener = centerOnChild.Content.DOLocalMove(origin + distance, duration);
                centeringTweener.SetEase(centerOnChild.SpeedCurve);
            }).AddTo(this.Disposer).AddTo(centerOnChild.Disposer);

            EventSystem.Receive<ChildsChangedEvent>().Where(evt => evt.Parent == centerOnChild.Content).Subscribe(_ =>
            {
                OnChildsChanged(entity, childsChangeDisposer, centeringTweener);
            }).AddTo(this.Disposer).AddTo(centerOnChild.Disposer);

            OnChildsChanged(entity, childsChangeDisposer, centeringTweener);
        }).AddTo(this.Disposer);
    }

    private void OnChildsChanged(IEntity entity, CompositeDisposable childsChangeDisposer, Tweener centeringTweener)
    {
        var centerOnChild = entity.GetComponent<CenterOnChild>();
        var viewComponent = entity.GetComponent<ViewComponent>();
        var draggedPositions = new List<Vector3>();
        var direction = Vector3.zero;
        var delta = Vector3.zero;

        if (centerOnChild.Content)
        {
            childsChangeDisposer.Clear();

            for (int i = 0; i < centerOnChild.Content.childCount + 1; i++)
            {
                var index = i;
                var target = index == centerOnChild.Content.childCount ? viewComponent.Transforms[0] : centerOnChild.Content.GetChild(index);

                target.OnPointerClickAsObservable().Where(eventData => !eventData.dragging && viewComponent.Transforms[0] != target).Subscribe(eventData =>
                {
                    centerOnChild.Index.Value = index;
                }).AddTo(this.Disposer).AddTo(centerOnChild.Disposer).AddTo(childsChangeDisposer);

                target.OnBeginDragAsObservable().Where(_ => centeringTweener != null).Subscribe(_ =>
                {
                    centeringTweener.Kill();
                    centeringTweener = null;
                    direction = Vector3.zero;
                    draggedPositions.Clear();
                    draggedPositions.Add(centerOnChild.Content.position);
                }).AddTo(this.Disposer).AddTo(centerOnChild.Disposer).AddTo(childsChangeDisposer);

                target.OnDragAsObservable().Subscribe(_ =>
                {
                    draggedPositions.Add(centerOnChild.Content.position);
                }).AddTo(this.Disposer).AddTo(centerOnChild.Disposer).AddTo(childsChangeDisposer);

                target.OnEndDragAsObservable().Subscribe(_ =>
                {
                    delta = draggedPositions.Count > 1 ? draggedPositions[draggedPositions.Count - 1] - draggedPositions[draggedPositions.Count - 2] : Vector3.zero;
                    for (int j = draggedPositions.Count - 1; j > 0; j--)
                    {
                        direction = Vector3.Normalize(draggedPositions[j] - draggedPositions[j - 1]);
                        if (direction != Vector3.zero) { break; }
                    }
                    centerOnChild.Index.SetValueAndForceNotify(FindSuitableChild(entity, direction, delta));
                }).AddTo(this.Disposer).AddTo(centerOnChild.Disposer).AddTo(childsChangeDisposer);
            }
        }
    }

    private int FindSuitableChild(IEntity entity, Vector3 direction, Vector3 delta)
    {
        var centerOnChild = entity.GetComponent<CenterOnChild>();
        var viewComponent = entity.GetComponent<ViewComponent>();

        var index = -1;
        var negativeIndex = -1;
        var minDistance = float.MaxValue;
        var negativeMinDistance = float.MaxValue;

        for (int i = 0; i < centerOnChild.Content.childCount; i++)
        {
            var target = centerOnChild.Content.GetChild(i);
            var distance = viewComponent.Transforms[0].position - target.position - delta * centerOnChild.Inertia;

            if (direction != Vector3.zero && Vector3.Dot(distance.normalized, direction) < 0)
            {
                if (index < 0 && negativeMinDistance > distance.magnitude)
                {
                    negativeIndex = i;
                    negativeMinDistance = distance.magnitude;
                }

                continue;
            }

            if (minDistance > distance.magnitude)
            {
                index = i;
                minDistance = distance.magnitude;
            }
        }

        return index < 0 ? negativeIndex : index;
    }
}
