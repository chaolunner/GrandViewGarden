using UnityEngine;
using UniEasy.ECS;
using Cinemachine;
using Common;
using UniRx;

public class ThridPersonCameraSystem : NetworkSystemBehaviour
{
    private IGroup ThridPersonCameraComponents;
    private IGroup PlayerControlComponents;
    private IGroup CrosshairComponents;
    private NetworkGroup Network;
    private INetworkTimeline NetwrokTimeline;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);
        ThridPersonCameraComponents = this.Create(typeof(ThridPersonCameraComponent), typeof(CinemachineVirtualCamera), typeof(CinemachineCollider), typeof(ViewComponent));
        PlayerControlComponents = this.Create(typeof(PlayerControlComponent), typeof(ShootComponent));
        CrosshairComponents = this.Create(typeof(CrosshairComponent), typeof(ViewComponent));
        Network = LockstepFactory.Create(PlayerControlComponents);
        NetwrokTimeline = Network.CreateTimeline(typeof(EventInput));
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        Network.OnUpdate += UpdateInputs;

        NetwrokTimeline.OnForward((entity, userInputData, deltaTime) =>
        {
            var isLocalPlayer = !entity.HasComponent<NetworkIdentityComponent>() || entity.GetComponent<NetworkIdentityComponent>().IsLocalPlayer;

            if (isLocalPlayer && ThridPersonCameraComponents.Entities.Count > 0)
            {
                var playerControlComponent = entity.GetComponent<PlayerControlComponent>();
                var shootComponent = entity.GetComponent<ShootComponent>();
                var thridPersonCameraComponent = ThridPersonCameraComponents.Entities[0].GetComponent<ThridPersonCameraComponent>();
                var virtualCamera = ThridPersonCameraComponents.Entities[0].GetComponent<CinemachineVirtualCamera>();
                var collider = ThridPersonCameraComponents.Entities[0].GetComponent<CinemachineCollider>();
                var transposer = virtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachineTransposer;

                virtualCamera.Follow = playerControlComponent.Follow;
                virtualCamera.LookAt = playerControlComponent.LookAt;

                if (playerControlComponent.Aim.Value == AimMode.Free)
                {
                    thridPersonCameraComponent.smoothTime = Mathf.Clamp01(thridPersonCameraComponent.smoothTime - thridPersonCameraComponent.Smooth * (float)deltaTime);
                }
                else if (playerControlComponent.Aim.Value == AimMode.Shoulder)
                {
                    thridPersonCameraComponent.smoothTime = Mathf.Clamp01(thridPersonCameraComponent.smoothTime + thridPersonCameraComponent.Smooth * (float)deltaTime);
                }
                if (playerControlComponent.Aim.Value == AimMode.AimDownSight)
                {
                    if (shootComponent.CurrentWeaponEntity != null)
                    {
                        var viewComponent = shootComponent.CurrentWeaponEntity.GetComponent<ViewComponent>();
                        transposer.m_FollowOffset = virtualCamera.Follow.InverseTransformDirection(viewComponent.Transforms[0].position - virtualCamera.Follow.position) + shootComponent.adsPosition;
                    }
                }
                else
                {
                    transposer.m_FollowOffset = Vector3.Lerp(thridPersonCameraComponent.FollowOffset[(int)AimMode.Free], thridPersonCameraComponent.FollowOffset[(int)AimMode.Shoulder], thridPersonCameraComponent.smoothTime);
                }
                if (CrosshairComponents.Entities.Count > 0)
                {
                    var crosshairComponent = CrosshairComponents.Entities[0].GetComponent<CrosshairComponent>();
                    var viewComponent = CrosshairComponents.Entities[0].GetComponent<ViewComponent>();
                    var rectTransform = viewComponent.Transforms[0] as RectTransform;

                    if (playerControlComponent.Aim.Value == AimMode.AimDownSight)
                    {
                        rectTransform.sizeDelta = Vector2.zero;
                    }
                    else
                    {
                        rectTransform.sizeDelta = Mathf.Lerp(crosshairComponent.Free, crosshairComponent.Shoulder, thridPersonCameraComponent.smoothTime) * Vector2.one;
                    }
                }
                collider.m_DistanceLimit = transposer.m_FollowOffset.magnitude;
            }
            return null;
        }).AddTo(this.Disposer);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        Network.OnUpdate -= UpdateInputs;
    }

    private void UpdateInputs()
    {
        if (ThridPersonCameraComponents.Entities.Count <= 0) { return; }

        var virtualCamera = ThridPersonCameraComponents.Entities[0].GetComponent<CinemachineVirtualCamera>();
        var viewComponent = ThridPersonCameraComponents.Entities[0].GetComponent<ViewComponent>();
        LockstepUtility.AddInput(new EventInput()
            .WithType(EventCode.PlayerCamera)
            .Add((FixVector3)viewComponent.Transforms[0].position)
            .Add((FixVector3)viewComponent.Transforms[0].forward)
            .Add((Fix64)virtualCamera.m_Lens.FarClipPlane));
    }
}
