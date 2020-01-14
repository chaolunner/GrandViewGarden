using UnityEngine;
using UniEasy.ECS;
using UniEasy.DI;
using Common;
using UniRx;

public class ShootSystem : NetworkSystemBehaviour
{
    [Inject]
    private IPoolFactory PoolFactory;

    public TextAsset Bullet;
    public TextAsset Weapon;

    private IGroup ShootComponents;
    private NetworkGroup Network;
    private INetworkTimeline NetwrokTimeline;
    private BulletDAO BulletDAO;
    private WeaponDAO WeaponDAO;

    private readonly int Shoot_b = Animator.StringToHash("Shoot_b");

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);

        ShootComponents = this.Create(typeof(PlayerControlComponent), typeof(NetworkIdentityComponent), typeof(ShootComponent), typeof(Animator));
        Network = LockstepFactory.Create(ShootComponents);
        NetwrokTimeline = Network.CreateTimeline(typeof(MouseInput), typeof(KeyInput));
        BulletDAO = new BulletDAO(Bullet);
        WeaponDAO = new WeaponDAO(Weapon);
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
                    shootComponent.weapon = PrefabFactory.Instantiate(prefab, shootComponent.Parent);
                    shootComponent.weapon.transform.localPosition = WeaponDAO.GetLocalPosition(name);
                    shootComponent.bulletPrefab = Resources.Load<GameObject>(BulletDAO.GetPath(WeaponDAO.GetBullet(name)));
                    shootComponent.adsPosition = WeaponDAO.GetADSPosition(name);
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
                for (int j = 0; j < 3; j++)
                {
                    PoolFactory.Create(prefab, networkIdentityComponent.Identity.UserId);
                }
            }
        }).AddTo(this.Disposer);

        NetwrokTimeline.OnReverse((entity, result) =>
        {
        }).AddTo(this.Disposer);

        NetwrokTimeline.OnForward(data =>
        {
            var networkIdentityComponent = data.Entity.GetComponent<NetworkIdentityComponent>();
            var playerControlComponent = data.Entity.GetComponent<PlayerControlComponent>();
            var shootComponent = data.Entity.GetComponent<ShootComponent>();
            var animator = data.Entity.GetComponent<Animator>();
            var userInputData = data.UserInputData[0];
            var mouseInput = userInputData[0].GetInput<MouseInput>();
            var keyInput = userInputData[1].GetInput<KeyInput>();

            if (mouseInput != null && keyInput != null)
            {
                animator.SetBool(Shoot_b, mouseInput.MouseButtons.Contains(0));
                if (mouseInput.MouseButtons.Contains(0) && shootComponent.cooldownTime <= 0)
                {
                    var entity = PoolFactory.Pop(shootComponent.bulletPrefab, data.TickId);
                    var bulletComponent = entity.GetComponent<BulletComponent>();
                    var viewComponent = entity.GetComponent<ViewComponent>();

                    viewComponent.Transforms[0].position = shootComponent.weapon.transform.position;
                    viewComponent.Transforms[0].rotation = Quaternion.LookRotation(shootComponent.weapon.transform.forward, shootComponent.weapon.transform.up);
                    bulletComponent.Velocity = shootComponent.speed * viewComponent.Transforms[0].forward;
                    shootComponent.cooldownTime = shootComponent.cooldown;
                }
                if (playerControlComponent.Aim.Value == AimMode.Free && keyInput.KeyCodes.Contains((int)KeyCode.LeftAlt)) { }
                else { shootComponent.weapon.transform.LookAt(playerControlComponent.LookAt, Vector3.up); }
            }
            shootComponent.cooldownTime -= data.DeltaTime;

            return null;
        }).AddTo(this.Disposer);
    }
}
