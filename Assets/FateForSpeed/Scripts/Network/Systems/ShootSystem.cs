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
                    PoolFactory.Create(prefab, shootComponent.Parent);
                    shootComponent.weapon = PoolFactory.Pop(prefab);
                    shootComponent.weapon.transform.localPosition = WeaponDAO.GetLocalPosition(name);
                    shootComponent.adsPosition = WeaponDAO.GetADSPosition(name);
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
                for (int j = 0; j < 10; j++)
                {
                    PoolFactory.Create(networkIdentityComponent.Identity.UserId, prefab);
                }
            }
        }).AddTo(this.Disposer);

        NetwrokTimeline.OnReverse((entity, result) =>
        {
        }).AddTo(this.Disposer);

        NetwrokTimeline.OnForward((entity, userInputData, deltaTime) =>
        {
            var playerControlComponent = entity.GetComponent<PlayerControlComponent>();
            var shootComponent = entity.GetComponent<ShootComponent>();
            var animator = entity.GetComponent<Animator>();
            var mouseInput = userInputData[0].Input as MouseInput;
            var keyInput = userInputData[1].Input as KeyInput;

            if (mouseInput != null && keyInput != null)
            {
                animator.SetBool(Shoot_b, mouseInput.MouseButtons.Contains(0));
                if (playerControlComponent.Aim.Value == AimMode.Free && keyInput.KeyCodes.Contains((int)KeyCode.LeftAlt)) { }
                else { shootComponent.weapon.transform.LookAt(playerControlComponent.LookAt, Vector3.up); }
            }

            return new IUserInputResult[0];
        }).AddTo(this.Disposer);
    }
}
