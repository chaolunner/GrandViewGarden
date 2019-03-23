using UnityEngine.UI;
using UniRx.Triggers;
using UniEasy.ECS;
using UniEasy;
using UniRx;

[ContextMenu("ScrollView/DragScrollRectByChildSystem")]
public class DragScrollRectByChildSystem : RuntimeSystem
{
    private IGroup scrollViews;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);

        scrollViews = this.Create(typeof(DragScrollRectByChild), typeof(ScrollRect));
    }

    public override void OnEnable()
    {
        base.OnEnable();

        scrollViews.OnAdd().Subscribe(entity =>
        {
            var dragScrollRectByChild = entity.GetComponent<DragScrollRectByChild>();
            var scrollRect = entity.GetComponent<ScrollRect>();
            var childsChangeDisposer = new CompositeDisposable();

            EventSystem.Receive<ChildsChangedEvent>().Where(evt => evt.Parent == scrollRect.content).Subscribe(_ =>
            {
                OnChildsChanged(entity, childsChangeDisposer);
            }).AddTo(this.Disposer).AddTo(dragScrollRectByChild.Disposer);

            OnChildsChanged(entity, childsChangeDisposer);
        }).AddTo(this.Disposer);
    }

    private void OnChildsChanged(IEntity entity, CompositeDisposable childsChangeDisposer)
    {
        var dragScrollRectByChild = entity.GetComponent<DragScrollRectByChild>();
        var scrollRect = entity.GetComponent<ScrollRect>();

        if (scrollRect.content)
        {
            childsChangeDisposer.Clear();

            for (int i = 0; i < scrollRect.content.childCount; i++)
            {
                var target = scrollRect.content.GetChild(i);

                target.OnBeginDragAsObservable().Subscribe(eventData =>
                {
                    scrollRect.OnBeginDrag(eventData);
                }).AddTo(this.Disposer).AddTo(dragScrollRectByChild.Disposer).AddTo(childsChangeDisposer);

                target.OnDragAsObservable().Subscribe(eventData =>
                {
                    scrollRect.OnDrag(eventData);
                }).AddTo(this.Disposer).AddTo(dragScrollRectByChild.Disposer).AddTo(childsChangeDisposer);

                target.OnEndDragAsObservable().Subscribe(eventData =>
                {
                    scrollRect.OnEndDrag(eventData);
                }).AddTo(this.Disposer).AddTo(dragScrollRectByChild.Disposer).AddTo(childsChangeDisposer);
            }
        }
    }
}
