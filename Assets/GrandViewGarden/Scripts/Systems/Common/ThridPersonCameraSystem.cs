using UnityEngine;
using UniEasy.ECS;
using Cinemachine;
using Common;
using UniRx;

public class ThridPersonCameraSystem : NetworkSystemBehaviour
{
    private IGroup ThridPersonCameraComponents;
    private IGroup PlayerControlComponents;
    private NetworkGroup Network;
    private INetworkTimeline NetwrokTimeline;
    private const float Smooth = 4;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);
        ThridPersonCameraComponents = this.Create(typeof(ThridPersonCameraComponent), typeof(CinemachineVirtualCamera), typeof(CinemachineCollider));
        PlayerControlComponents = this.Create(typeof(PlayerControlComponent), typeof(ShootComponent));
        Network = LockstepFactory.Create(PlayerControlComponents);
        NetwrokTimeline = Network.CreateTimeline(typeof(EventInput));
    }

    public override void OnEnable()
    {
        base.OnEnable();

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
                    thridPersonCameraComponent.smoothTime = Mathf.Clamp01(thridPersonCameraComponent.smoothTime - Smooth * (float)deltaTime);
                }
                else if (playerControlComponent.Aim.Value == AimMode.Shoulder)
                {
                    thridPersonCameraComponent.smoothTime = Mathf.Clamp01(thridPersonCameraComponent.smoothTime + Smooth * (float)deltaTime);
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
                collider.m_DistanceLimit = transposer.m_FollowOffset.magnitude;
            }
            return null;
        }).AddTo(this.Disposer);
    }
}
