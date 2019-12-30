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

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);

        ShootComponents = this.Create(typeof(NetworkIdentityComponent), typeof(ShootComponent));
        Network = LockstepFactory.Create(ShootComponents);
        NetwrokTimeline = Network.CreateTimeline(typeof(MouseInput));
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

            for (int i = 0; i < shootComponent.Weapon.Count; i++)
            {
                var path = BulletDAO.GetPath(WeaponDAO.GetBullet(shootComponent.Weapon[i]));
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
            var mouseInput = userInputData[0].Input as MouseInput;

            if (mouseInput != null && mouseInput.MouseButtons.Contains(0))
            {

            }

            return new IUserInputResult[0];
        }).AddTo(this.Disposer);
    }
}
