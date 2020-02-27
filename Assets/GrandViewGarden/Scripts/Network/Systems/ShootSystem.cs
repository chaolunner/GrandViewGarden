using System.Collections;
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
    private NetworkGroup Network;
    private INetworkTimeline NetwrokTimeline;
    private BulletDAO BulletDAO;
    private WeaponDAO WeaponDAO;

    private readonly int Shoot_b = Animator.StringToHash("Shoot_b");
    private const string NameStr = "Name";

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);
        ShootComponents = this.Create(typeof(PlayerControlComponent), typeof(NetworkIdentityComponent), typeof(ShootComponent), typeof(Animator), typeof(ViewComponent));
        Network = LockstepFactory.Create(ShootComponents);
        NetwrokTimeline = Network.CreateTimeline(typeof(MouseInput), typeof(KeyInput), typeof(EventInput));
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
                    shootComponent.bulletPrefab = Resources.Load<GameObject>(BulletDAO.GetPath(bullet));
                    shootComponent.muzzleFlashesPrefab = Resources.Load<GameObject>(WeaponDAO.GetMuzzleFlashesEffectPath(name));
                    shootComponent.adsPosition = WeaponDAO.GetADSPosition(name);
                    shootComponent.bulletLocalPosition = WeaponDAO.GetBulletSpawnPosition(name);
                    shootComponent.muzzleFlashesPosition = WeaponDAO.GetMuzzleFlashesPosition(name);
                    shootComponent.holeSize = BulletDAO.GetHoleSize(bullet);
                    shootComponent.speed = WeaponDAO.GetSpeed(name);
                    shootComponent.cooldown = WeaponDAO.GetCooldown(name);

                    if (shootComponent.CurrentWeaponEntity != null)
                    {
                        var weaponViewComponent = shootComponent.CurrentWeaponEntity.GetComponent<ViewComponent>();

                        weaponViewComponent.Transforms[0].localPosition = WeaponDAO.GetPosition(name);
                        shootComponent.weaponLocalRotation = Quaternion.Euler(new Vector3(0, 90, 90));
                        weaponViewComponent.Transforms[0].localRotation = shootComponent.weaponLocalRotation;
                    }
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
            var weaponViewComponent = shootComponent.CurrentWeaponEntity != null ? shootComponent.CurrentWeaponEntity.GetComponent<ViewComponent>() : null;
            var userInputData = data.UserInputData[0];
            var mouseInput = userInputData[0].GetInput<MouseInput>();
            var keyInput = userInputData[1].GetInput<KeyInput>();
            var eventInputs = userInputData[2].GetInputs<EventInput>();

            EventInput eventInput = null;

            for (int i = 0; i < eventInputs.Length; i++)
            {
                if (eventInputs[i].Type == EventCode.PlayerCamera)
                {
                    eventInput = eventInputs[i];
                    break;
                }
            }

            if (mouseInput != null && keyInput != null)
            {
                if (playerControlComponent.Aim.Value == AimMode.Free && keyInput.KeyCodes.Contains((int)KeyCode.LeftAlt)) { }
                else if (weaponViewComponent != null && eventInput != null)
                {
                    // The point hit by the muzzle is different from the point hit in the center of the screen.
                    // So how to solve this problem, my solution is like this:

                    // First, get the hit point in the center of the screen.
                    // 0 - camera position, 1 - camera dirstion, 2 - camera far clip plane.
                    RaycastHit hit;
                    var ray = new Ray((Vector3)eventInput.Get<FixVector3>(0), (Vector3)eventInput.Get<FixVector3>(1));
                    var point = Physics.Raycast(ray, out hit, (float)eventInput.Get<Fix64>(2)) ? hit.point : ray.origin + (float)eventInput.Get<Fix64>(2) * ray.direction;

                    // Reset rotation to default rotation
                    weaponViewComponent.Transforms[0].localRotation = shootComponent.weaponLocalRotation;

                    // If not in aim down sight mode, then I can fine-tune the angle of the gun to make the direction of
                    // the bullet closer to the point at the center of the screen 
                    if (playerControlComponent.Aim.Value != AimMode.AimDownSight)
                    {
                        var targetDirection = (point - weaponViewComponent.Transforms[0].TransformPoint(shootComponent.bulletLocalPosition)).normalized;
                        var angle = Vector3.Angle(weaponViewComponent.Transforms[0].forward, targetDirection);
                        var t = angle == 0 ? 1 : Mathf.Clamp01(shootComponent.LimitAngle / angle);
                        var rotation = Quaternion.LookRotation(Vector3.Lerp(weaponViewComponent.Transforms[0].forward, targetDirection, t));

                        weaponViewComponent.Transforms[0].rotation = rotation;
                    }
                }

                animator.SetBool(Shoot_b, mouseInput.MouseButtons.Contains(0));

                if (mouseInput.MouseButtons.Contains(0) && shootComponent.cooldownTime <= 0)
                {
                    var entity = PoolFactory.Spawn(shootComponent.bulletPrefab, networkIdentityComponent.Identity.UserId, data.TickId);
                    var bulletComponent = entity.GetComponent<BulletComponent>();
                    var bulletViewComponent = entity.GetComponent<ViewComponent>();

                    if (weaponViewComponent != null)
                    {
                        bulletViewComponent.Transforms[0].position = weaponViewComponent.Transforms[0].TransformPoint(shootComponent.bulletLocalPosition);
                        bulletViewComponent.Transforms[0].rotation = Quaternion.LookRotation(weaponViewComponent.Transforms[0].forward, weaponViewComponent.Transforms[0].up);
                        bulletComponent.velocity = shootComponent.speed * (FixVector3)bulletViewComponent.Transforms[0].forward;
                        bulletComponent.holeSize = shootComponent.holeSize;

                        StartCoroutine(AsyncMuzzleFlashes(shootComponent.muzzleFlashesPrefab, weaponViewComponent.Transforms[0].TransformPoint(shootComponent.muzzleFlashesPosition), weaponViewComponent.Transforms[0].rotation));
                    }

                    shootComponent.cooldownTime = shootComponent.cooldown;
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
