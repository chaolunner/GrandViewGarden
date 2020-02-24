using UnityEngine;

public struct MaterialData
{
    public GameObject BulletHole;
    public float DetectionDepth;
    public GameObject ImpactEffect;
    public float ImpactSize;

    public MaterialData(GameObject bulletHole, float detectionDepth, GameObject impactEffect, float impactSize)
    {
        BulletHole = bulletHole;
        DetectionDepth = detectionDepth;
        ImpactEffect = impactEffect;
        ImpactSize = impactSize;
    }
}
