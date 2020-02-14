using UnityEngine;
using UniEasy.ECS;
using Cinemachine;
using UniRx;

public class ThridPersonCameraSystem : NetworkSystemBehaviour
{
    private IGroup ThridPersonCameraComponents;
    private IGroup PlayerControlComponents;
    private const float SmoothTime = 1f;
    private const float AimDownSightSmooth = 10f;

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);
        ThridPersonCameraComponents = this.Create(typeof(ThridPersonCameraComponent), typeof(CinemachineVirtualCamera), typeof(CinemachineCollider));
        PlayerControlComponents = this.Create(typeof(PlayerControlComponent));
    }

    public override void OnEnable()
    {
        base.OnEnable();
        PlayerControlComponents.OnAdd().Subscribe(entity1 =>
        {
            var playerControlComponent = entity1.GetComponent<PlayerControlComponent>();
            var lastAimMode = playerControlComponent.Aim.Value;

            if (!entity1.HasComponent<NetworkIdentityComponent>() || (entity1.HasComponent<NetworkIdentityComponent>() && entity1.GetComponent<NetworkIdentityComponent>().IsLocalPlayer))
            {
                foreach (var entity2 in ThridPersonCameraComponents.Entities)
                {
                    var virtualCamera = entity2.GetComponent<CinemachineVirtualCamera>();
                    var collider = entity2.GetComponent<CinemachineCollider>();
                    var transposer = virtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachineTransposer;

                    virtualCamera.Follow = playerControlComponent.Follow;
                    virtualCamera.LookAt = playerControlComponent.LookAt;
                    collider.m_DistanceLimit = transposer.m_FollowOffset.magnitude;
                }
            }

            playerControlComponent.Aim.DistinctUntilChanged().Subscribe(mode =>
            {
                if (!entity1.HasComponent<NetworkIdentityComponent>() || (entity1.HasComponent<NetworkIdentityComponent>() && entity1.GetComponent<NetworkIdentityComponent>().IsLocalPlayer))
                {
                    foreach (var entity2 in ThridPersonCameraComponents.Entities)
                    {
                        var thridPersonCameraComponent = entity2.GetComponent<ThridPersonCameraComponent>();
                        var virtualCamera = entity2.GetComponent<CinemachineVirtualCamera>();
                        var transposer = virtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachineTransposer;
                        var index = (int)mode;
                        var smoothTime = 0f;

                        thridPersonCameraComponent.smoothDisposer.Clear();

                        if (lastAimMode == AimMode.Shoulder || mode == AimMode.Shoulder)
                        {
                            Observable.EveryUpdate().Subscribe(_ =>
                            {
                                transposer.m_FollowOffset = Vector3.Lerp(transposer.m_FollowOffset, thridPersonCameraComponent.FollowOffset[index], Mathf.Clamp01(smoothTime / SmoothTime));
                                if (transposer.m_FollowOffset == thridPersonCameraComponent.FollowOffset[index])
                                {
                                    thridPersonCameraComponent.smoothDisposer.Clear();
                                }
                                smoothTime += Time.deltaTime;
                            }).AddTo(this.Disposer).AddTo(playerControlComponent.Disposer).AddTo(thridPersonCameraComponent.smoothDisposer);
                        }
                        else if (mode == AimMode.AimDownSight && entity1.HasComponent<ShootComponent>())
                        {
                            var shootComponent = entity1.GetComponent<ShootComponent>();
                            var targetPosition = virtualCamera.Follow.InverseTransformDirection(shootComponent.weapon.transform.position - virtualCamera.Follow.position) + shootComponent.adsPosition;

                            transposer.m_FollowOffset = targetPosition;
                            Observable.EveryUpdate().Subscribe(_ =>
                            {
                                targetPosition = virtualCamera.Follow.InverseTransformDirection(shootComponent.weapon.transform.position - virtualCamera.Follow.position) + shootComponent.adsPosition;
                                transposer.m_FollowOffset = Vector3.Lerp(transposer.m_FollowOffset, targetPosition, AimDownSightSmooth * Time.deltaTime);
                            }).AddTo(this.Disposer).AddTo(playerControlComponent.Disposer).AddTo(thridPersonCameraComponent.smoothDisposer);
                        }
                        else
                        {
                            transposer.m_FollowOffset = thridPersonCameraComponent.FollowOffset[index];
                        }
                    }
                }
                lastAimMode = mode;
            }).AddTo(this.Disposer).AddTo(playerControlComponent.Disposer);
        }).AddTo(this.Disposer);
    }
}
