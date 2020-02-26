using System.Collections.Generic;
using System.Collections;
using ch.sycoforge.Decal;
using UnityEngine;
using UniEasy.ECS;
using UniEasy.DI;
using Common;
using UniRx;

public class BulletSystem : NetworkSystemBehaviour
{
    public TextAsset Material;

    [Inject]
    private IPoolFactory PoolFactory;
    private IGroup BulletComponents;
    private NetworkGroup Network;
    private INetworkTimeline NetwrokTimeline;
    private MaterialDAO MaterialDAO;
    private Dictionary<string, MaterialData> materialDict = new Dictionary<string, MaterialData>();

    private const string MaterialStr = "Material";
    private const string DefaultStr = "Default";

    public override void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory, PrefabFactory prefabFactory)
    {
        base.Initialize(eventSystem, poolManager, groupFactory, prefabFactory);
        BulletComponents = this.Create(typeof(BulletComponent), typeof(NetworkIdentityComponent), typeof(ViewComponent), typeof(CapsuleCollider));
        Network = LockstepFactory.Create(BulletComponents, usePhysics: true);
        NetwrokTimeline = Network.CreateTimeline(typeof(EventInput));
        InitializeMaterials();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        BulletComponents.OnAdd().Subscribe(entity =>
        {
            var bulletComponent = entity.GetComponent<BulletComponent>();
            var viewComponent = entity.GetComponent<ViewComponent>();
            var capsuleCollider = entity.GetComponent<CapsuleCollider>();

            bulletComponent.radius = 0.5f * Mathf.Max(2 * capsuleCollider.radius, capsuleCollider.height);
        }).AddTo(this.Disposer);

        NetwrokTimeline.OnForward(data =>
        {
            var bulletComponent = data.Entity.GetComponent<BulletComponent>();
            var viewComponent = data.Entity.GetComponent<ViewComponent>();
            var capsuleCollider = data.Entity.GetComponent<CapsuleCollider>();

            if (bulletComponent.velocity == FixVector3.zero) { return null; }

            var direction = (FixVector3)viewComponent.Transforms[0].forward;
            var offset = (FixVector3)capsuleCollider.center + bulletComponent.radius * direction;
            var origin = (FixVector3)viewComponent.Transforms[0].position + offset;
            var maxDistance = bulletComponent.velocity.magnitude * data.DeltaTime;

            RaycastHit hit;

            if (Physics.Raycast((Vector3)origin, (Vector3)direction, out hit, (float)maxDistance))
            {
                viewComponent.Transforms[0].position = (Vector3)(origin + hit.distance * direction - offset);
                // Here we cannot directly use hit.normal as the direction,
                // because our collider may be simplified and does not match the model,
                // our scaling on the y-axis is relatively large to ensure that the decal can be printed on the model.
                EasyDecal.ProjectAt(GetMaterial(hit.collider.name).BulletHole, null, hit.point, viewComponent.Transforms[0].forward, 0, new Vector3(bulletComponent.holeSize, GetMaterial(hit.collider.name).DetectionDepth, bulletComponent.holeSize));
                StartCoroutine(AsyncImpectEffect(hit.collider.name, hit.point, Quaternion.identity));
                bulletComponent.velocity = FixVector3.zero;
                PoolFactory.Despawn(data.Entity);
            }
            else
            {
                viewComponent.Transforms[0].position = (Vector3)(origin + maxDistance * direction - offset);
            }
            return null;
        }).AddTo(this.Disposer);
    }

    private void InitializeMaterials()
    {
        MaterialDAO = new MaterialDAO(Material, MaterialStr);

        var materials = MaterialDAO.GetColunm(MaterialStr);

        for (int i = 0; i < materials.Length; i++)
        {
            if (materialDict.ContainsKey(materials[i])) { continue; }

            var bulletHole = Resources.Load<GameObject>(MaterialDAO.GetBulletHole(materials[i]));
            var detectionDepth = MaterialDAO.GetDetectionDepth(materials[i]);
            var impactEffect = Resources.Load<GameObject>(MaterialDAO.GetImpactEffect(materials[i]));
            var impactSize = MaterialDAO.GetImpactSize(materials[i]);

            materialDict.Add(materials[i], new MaterialData(bulletHole, detectionDepth, impactEffect, impactSize));
        }
    }

    private MaterialData GetMaterial(string material = DefaultStr)
    {
        if (materialDict.ContainsKey(material))
        {
            return materialDict[material];
        }
        if (materialDict.ContainsKey(DefaultStr))
        {
            return materialDict[DefaultStr];
        }
        return default;
    }

    private IEnumerator AsyncImpectEffect(string material, Vector3 position, Quaternion rotation)
    {
        var entity = PoolFactory.Spawn(GetMaterial(material).ImpactEffect, position, rotation);
        var viewComponent = entity.GetComponent<ViewComponent>();

        viewComponent.Transforms[0].localScale = GetMaterial(material).ImpactSize * Vector3.one;
        yield return new WaitForSeconds(1);
        PoolFactory.Despawn(entity);
    }
}
