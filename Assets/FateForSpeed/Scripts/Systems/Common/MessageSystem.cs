using UnityEngine.UI;
using UniEasy.ECS;
using UnityEngine;
using System.Linq;
using UniRx;
using TMPro;

public class MessageSystem : SystemBehaviour
{
    private IGroup MessageComponents;
    private IGroup MessageLineComponents;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);

        MessageComponents = this.Create(typeof(GridLayoutGroup), typeof(MessageComponent), typeof(ViewComponent));
        MessageLineComponents = this.Create(typeof(TextMeshProUGUI), typeof(MessageLineComponent), typeof(ViewComponent));
    }

    public override void OnEnable()
    {
        base.OnEnable();

        MessageComponents.OnAdd().Subscribe(entity =>
        {
            var messageComponent = entity.GetComponent<MessageComponent>();
            var viewComponent = entity.GetComponent<ViewComponent>();

            for (int i = 0; i < messageComponent.MaxCount; i++)
            {
                var go = PrefabFactory.Instantiate(messageComponent.MessageLine, viewComponent.Transforms[0]);
                go.SetActive(false);
            }

            EventSystem.Receive<MessageEvent>().ObserveOnMainThread().Subscribe(evt =>
            {
                var messageLineEntity = MessageLineComponents.Entities.OrderBy(e => e.GetComponent<MessageLineComponent>().Time.Value).FirstOrDefault();
                var messageLineComponent = messageLineEntity.GetComponent<MessageLineComponent>();
                var text = messageLineEntity.GetComponent<TextMeshProUGUI>();

                messageLineEntity.GetComponent<ViewComponent>().Transforms[0].SetAsLastSibling();
                messageLineComponent.Time.Value = evt.Duration;
                if (evt.Type == LogType.Warning)
                {
                    text.text = "<color=#FFB900>" + evt.Message + "</color>";
                }
                else if (evt.Type == LogType.Error)
                {
                    text.text = "<color=#EA0043>" + evt.Message + "</color>";
                }
                else
                {
                    text.text = evt.Message;
                }
            }).AddTo(this.Disposer).AddTo(messageComponent.Disposer);
        }).AddTo(this.Disposer);

        MessageLineComponents.OnAdd().Subscribe(entity =>
        {
            var messageLineComponent = entity.GetComponent<MessageLineComponent>();
            var text = entity.GetComponent<TextMeshProUGUI>();
            var viewComponent = entity.GetComponent<ViewComponent>();

            messageLineComponent.Time.DistinctUntilChanged().Where(t => !viewComponent.Transforms[0].gameObject.activeSelf && t > 0).Subscribe(t =>
            {
                text.alpha = 1;
                viewComponent.Transforms[0].gameObject.SetActive(true);

                Observable.EveryUpdate().TakeWhile(_ => viewComponent.Transforms[0].gameObject.activeSelf && messageLineComponent.Time.Value > 0).Subscribe(_ =>
                {
                    messageLineComponent.Time.Value -= Time.deltaTime;
                    text.alpha = Mathf.Clamp01(messageLineComponent.Time.Value);
                    if (messageLineComponent.Time.Value <= 0) { viewComponent.Transforms[0].gameObject.SetActive(false); }
                }).AddTo(this.Disposer).AddTo(messageLineComponent.Disposer);
            }).AddTo(this.Disposer).AddTo(messageLineComponent.Disposer);
        }).AddTo(this.Disposer);
    }
}
