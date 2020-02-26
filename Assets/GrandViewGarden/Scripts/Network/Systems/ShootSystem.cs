﻿using System.Collections;
using UnityEngine;
using UniEasy.ECS;
using Cinemachine;
using UniEasy.DI;
using Common;
using UniRx;

public class ShootSystem : NetworkSystemBehaviour
{
    [Inject]
    private IPoolFactory PoolFactory;

    public TextAsset Bullet;
    public TextAsset Weapon;
    [Range(0, 100)]
    public int WarmupBullets = 10;

    private IGroup ShootComponents;
    private IGroup ThridPersonCameraComponents;
    private NetworkGroup Network;
    private INetworkTimeline NetwrokTimeline;
    private BulletDAO BulletDAO;
    private WeaponDAO WeaponDAO;

    private readonly int Shoot_b = Animator.StringToHash("Shoot_b");
    private const string NameStr = "Name";

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);
        ShootComponents = this.Create(typeof(PlayerControlComponent), typeof(NetworkIdentityComponent), typeof(ShootComponent), typeof(Animator));
        ThridPersonCameraComponents = this.Create(typeof(ThridPersonCameraComponent), typeof(CinemachineVirtualCamera), typeof(ViewComponent));
        Network = LockstepFactory.Create(ShootComponents);
        NetwrokTimeline = Network.CreateTimeline(typeof(MouseInput), typeof(KeyInput));
        BulletDAO = new BulletDAO(Bullet, NameStr);
        WeaponDAO = new WeaponDAO(Weapon, NameStr);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        ShootComponents.OnAdd().Subscribe(entity =>
        {
            var networkIdentityComponent = entity.GetComponent<NetworkIdentityComponent>();
            var shootComponent = entity.GetComponent<ShootComponent>();

            shootComponent.WeaponIndex.DistinctUntilChanged().Subscribe(index =>
            {
                if (index >= 0 && index < shootComponent.Weapons.Count)
                {
                    var name = shootComponent.Weapons[index];
                    var path = WeaponDAO.GetPath(name);
                    var prefab = Resources.Load<GameObject>(path);
                    var bullet = WeaponDAO.GetBullet(name);

                    if (shootComponent.CurrentWeaponEntity != null)
                    {
                        PoolFactory.Despawn(shootComponent.CurrentWeaponEntity);
                    }

                    shootComponent.CurrentWeaponEntity = PoolFactory.Spawn(prefab, shootComponent.Parent);

                    if (shootComponent.CurrentWeaponEntity != null)
                    {
                        shootComponent.CurrentWeaponEntity.GetComponent<ViewComponent>().Transforms[0].localPosition = WeaponDAO.GetPosition(name);
                    }

                    shootComponent.bulletPrefab = Resources.Load<GameObject>(BulletDAO.GetPath(bullet));
                    shootComponent.muzzleFlashesPrefab = Resources.Load<GameObject>(WeaponDAO.GetMuzzleFlashesEffectPath(name));
                    shootComponent.adsPosition = WeaponDAO.GetADSPosition(name);
                    shootComponent.bulletPosition = WeaponDAO.GetBulletSpawnPosition(name);
                    shootComponent.muzzlePosition = WeaponDAO.GetMuzzlePosition(name);
                    shootComponent.holeSize = BulletDAO.GetHoleSize(bullet);
                    shootComponent.speed = WeaponDAO.GetSpeed(name);
                    shootComponent.cooldown = WeaponDAO.GetCooldown(name);
                }
                else
                {
                    shootComponent.WeaponIndex.Value = Mathf.Clamp(index, 0, shootComponent.Weapons.Count);
                }
            }).AddTo(this.Disposer).AddTo(shootComponent.Disposer);

            for (int i = 0; i < shootComponent.Weapons.Count; i++)
            {
                var path = BulletDAO.GetPath(WeaponDAO.GetBullet(shootComponent.Weapons[i]));
                var prefab = Resources.Load<GameObject>(path);
                for (int j = 0; j < WarmupBullets; j++)
                {
                    PoolFactory.Despawn(PoolFactory.Spawn(prefab, networkIdentityComponent.Identity.UserId, 0));
                }
            }
        }).AddTo(this.Disposer);

        NetwrokTimeline.OnForward(data =>
        {
            var networkIdentityComponent = data.Entity.GetComponent<NetworkIdentityComponent>();
            var playerControlComponent = data.Entity.GetComponent<PlayerControlComponent>();
            var shootComponent = data.Entity.GetComponent<ShootComponent>();
            var animator = data.Entity.GetComponent<Animator>();
            var viewComponent = shootComponent.CurrentWeaponEntity != null ? shootComponent.CurrentWeaponEntity.GetComponent<ViewComponent>() : null;
            var userInputData = data.UserInputData[0];
            var mouseInput = userInputData[0].GetInput<MouseInput>();
            var keyInput = userInputData[1].GetInput<KeyInput>();

            if (mouseInput != null && keyInput != null)
            {
                animator.SetBool(Shoot_b, mouseInput.MouseButtons.Contains(0));
                if (mouseInput.MouseButtons.Contains(0) && shootComponent.cooldownTime <= 0)
                {
                    var entity = PoolFactory.Spawn(shootComponent.bulletPrefab, networkIdentityComponent.Identity.UserId, data.TickId);
                    var bulletComponent = entity.GetComponent<BulletComponent>();
                    var bulletViewComponent = entity.GetComponent<ViewComponent>();

                    if (viewComponent != null)
                    {
                        bulletViewComponent.Transforms[0].position = viewComponent.Transforms[0].TransformPoint(shootComponent.bulletPosition);
                        bulletViewComponent.Transforms[0].rotation = Quaternion.LookRotation(viewComponent.Transforms[0].forward, viewComponent.Transforms[0].up);
                        bulletComponent.velocity = shootComponent.speed * (FixVector3)bulletViewComponent.Transforms[0].forward;
                        bulletComponent.holeSize = shootComponent.holeSize;

                        StartCoroutine(AsyncMuzzleFlashes(shootComponent.muzzleFlashesPrefab, viewComponent.Transforms[0].TransformPoint(shootComponent.muzzlePosition), viewComponent.Transforms[0].rotation));
                    }

                    shootComponent.cooldownTime = shootComponent.cooldown;
                }
                if (playerControlComponent.Aim.Value == AimMode.Free && keyInput.KeyCodes.Contains((int)KeyCode.LeftAlt)) { }
                else if (viewComponent != null && ThridPersonCameraComponents.Entities.Count > 0)
                {
                    var virtualCamera = ThridPersonCameraComponents.Entities[0].GetComponent<CinemachineVirtualCamera>();
                    var cameraTransform = ThridPersonCameraComponents.Entities[0].GetComponent<ViewComponent>().Transforms[0];

                    Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, virtualCamera.m_Lens.FarClipPlane))
                    {
                        viewComponent.Transforms[0].LookAt(hit.point);
                    }
                    else
                    {
                        viewComponent.Transforms[0].LookAt(ray.origin + virtualCamera.m_Lens.FarClipPlane * ray.direction);
                    }
                }
            }
            shootComponent.cooldownTime -= data.DeltaTime;

            return null;
        }).AddTo(this.Disposer);
    }

    private IEnumerator AsyncMuzzleFlashes(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        var entity = PoolFactory.Spawn(prefab, position, rotation);
        yield return new WaitForSeconds(0.1f);
        PoolFactory.Despawn(entity);
    }
}
