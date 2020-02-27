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

#if UNITY_EDITOR
        if (decal.Lifetime < 1)
        {
            Debug.LogWarning("Do not set the life time too short, this may affect the fade out !");
        }
#endif

        decal.DontDestroy = true;
        decal.Reset(true);
        decal.LateBake();
        decal.OnFadedOut += OnFadedOut;

        decalDict.Add(decal, entity);
        return decal;
    }

    private void OnFadedOut(EasyDecal decal)
    {
        decal.transform.SetParent(null);
        decal.OnFadedOut -= OnFadedOut;
        PoolFactory.Despawn(decalDict[decal]);
        decalDict.Remove(decal);
    }
}
