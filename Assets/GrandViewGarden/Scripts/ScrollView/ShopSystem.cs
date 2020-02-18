using System.Collections;
using UniRx.Triggers;
using UniEasy.ECS;
using UnityEngine;
using UniRx;

public class ShopSystem : SystemBehaviour
{
    public ShopView ShopView;
    public GameObject ShopElementPrefab;
    private IGroup ShopElementComponents;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);
        ShopElementComponents = this.Create(typeof(ShopElement), typeof(ViewComponent));
    }

    public override void OnEnable()
    {
        base.OnEnable();

        var onElementCountChanged = ShopView.Elements.ObserveEveryValueChanged(elements => elements.Count).ThrottleFirstFrame(1).Select(_ => true);
        var onDataCountChanged = ShopView.Data.ObserveEveryValueChanged(data => data.Count).Select(_ => true);
        var onScrolling = ShopView.Scrollbar.OnValueChangedAsObservable().Select(_ => true);

        onElementCountChanged.Merge(onDataCountChanged).Merge(onScrolling).Where(_ => ShopView.gameObject.activeSelf).Subscribe(_ =>
        {
            ShopView.Scroll(ShopView.Data.ToArray(), ShopView.Scrollbar.value, ShopView.ElementSize);
        }).AddTo(this.Disposer).AddTo(ShopView);

        ShopElementComponents.OnAdd().Subscribe(entity =>
        {
            var showElement = entity.GetComponent<ShopElement>();
            var viewComponent = entity.GetComponent<ViewComponent>();
            var rectTransform = viewComponent.Transforms[0] as RectTransform;

            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, ShopView.ElementSize);

            showElement.OnPointerClickAsObservable().Subscribe(_ =>
            {
            }).AddTo(this.Disposer).AddTo(ShopView).AddTo(viewComponent.Disposer);
        }).AddTo(this.Disposer).AddTo(ShopView);
    }

    private IEnumerator Start()
    {
        yield return null;

        for (int i = 0; i < ShopView.GetElementCount(); i++)
        {
            var go = PrefabFactory.Instantiate(ShopElementPrefab, ShopView.Content);
            ShopView.Elements.Add(go.GetComponent<ShopElement>());
        }
    }
}
