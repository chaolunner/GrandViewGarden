using UnityEngine;
using UniEasy.ECS;
using Common;
using UniRx;

public class CrosshairSystem : NetworkSystemBehaviour
{
    private IGroup CrosshairComponents;
    private IGroup PlayerControlComponents;
    private NetworkGroup Network;
    private INetworkTimeline NetwrokTimeline;
    private const float Smooth = 4;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);
        CrosshairComponents = this.Create(typeof(CrosshairComponent), typeof(ViewComponent));
        PlayerControlComponents = this.Create(typeof(PlayerControlComponent));
        Network = LockstepFactory.Create(PlayerControlComponents);
        NetwrokTimeline = Network.CreateTimeline(typeof(EventInput));
    }

    public override void OnEnable()
    {
        base.OnEnable();

        NetwrokTimeline.OnForward((entity, userInputData, deltaTime) =>
        {
            var isLocalPlayer = !entity.HasComponent<NetworkIdentityComponent>() || entity.GetComponent<NetworkIdentityComponent>().IsLocalPlayer;

            if (isLocalPlayer && CrosshairComponents.Entities.Count > 0)
            {
                var crosshairComponent = CrosshairComponents.Entities[0].GetComponent<CrosshairComponent>();
                var viewComponent = CrosshairComponents.Entities[0].GetComponent<ViewComponent>();
                var playerControlComponent = entity.GetComponent<PlayerControlComponent>();
                var rectTransform = viewComponent.Transforms[0] as RectTransform;

                if (playerControlComponent.Aim.Value == AimMode.Free)
                {
                    crosshairComponent.smoothTime = Mathf.Clamp01(crosshairComponent.smoothTime - Smooth * (float)deltaTime);
                }
                else if (playerControlComponent.Aim.Value == AimMode.Shoulder)
                {
                    crosshairComponent.smoothTime = Mathf.Clamp01(crosshairComponent.smoothTime + Smooth * (float)deltaTime);
                }
                if (playerControlComponent.Aim.Value == AimMode.AimDownSight)
                {
                    rectTransform.sizeDelta = Vector2.zero;
                }
                else
                {
                    rectTransform.sizeDelta = Mathf.Lerp(crosshairComponent.Free, crosshairComponent.Shoulder, crosshairComponent.smoothTime) * Vector2.one;
                }
            }

            return null;
        }).AddTo(this.Disposer);
    }
}
