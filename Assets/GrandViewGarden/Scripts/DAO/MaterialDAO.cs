using UnityEngine;

public class MaterialDAO : CsvDAO
{
    private const string BulletHoleStr = "BulletHole";
    private const string DetectionDepthStr = "DetectionDepth";
    private const string ImpactEffectStr = "ImpactEffect";
    private const string ImpactSizeStr = "ImpactSize";

    public MaterialDAO(TextAsset textAsset, string name) : base(textAsset, name) { }

    public string GetBulletHole(string name)
    {
        return GetString(name, BulletHoleStr);
    }

    public float GetDetectionDepth(string name)
    {
        return GetFloat(name, DetectionDepthStr);
    }

    public string GetImpactEffect(string name)
    {
        return GetString(name, ImpactEffectStr);
    }

    public float GetImpactSize(string name)
    {
        return GetFloat(name, ImpactSizeStr);
    }
}
