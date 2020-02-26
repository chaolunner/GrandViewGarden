using System.Collections.Generic;
using ch.sycoforge.Decal;
using UnityEngine;
using UniEasy.ECS;
using UniEasy.DI;

public class DecalSystem : SystemBehaviour
{
    [Inject]
    private IPoolFactory PoolFactory;
    private Dictionary<EasyDecal, IEntity> decalDict = new Dictionary<EasyDecal, IEntity>();

    public override void OnEnable()
    {
        base.OnEnable();
        EasyDecal.Instantiation = PoolInstantiation;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        EasyDecal.Instantiation = null;
    }

    private EasyDecal PoolInstantiation(GameObject decalPrefab, GameObject parent, Vector3 position, Quaternion rotation)
    {
        var entity = PoolFactory.Spawn(decalPrefab, position, rotation, parent ? parent.transform : null);
        var decal = entity.GetComponent<EasyDecal>();

        decalDict.Add(decal, entity);

        decal.DontDestroy = true;
        decal.Reset(true);

        decal.OnFadedOut += OnFadedOut;

        return decal;
    }

    private void OnFadedOut(EasyDecal decal)
    {
        decal.transform.SetParent(null);
        PoolFactory.Despawn(decalDict[decal]);
        decalDict.Remove(decal);
        decal.OnFadedOut -= OnFadedOut;
    }
}
