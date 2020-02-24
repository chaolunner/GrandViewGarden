using UnityEngine;

public class WeaponDAO : CsvDAO
{
    private const string PathStr = "Path";
    private const string PositionStr = "Position";
    private const string ADSStr = "ADS";
    private const string BulletStr = "Bullet";
    private const string SpeedStr = "Speed";
    private const string CooldownStr = "Cooldown";
    private const string MuzzleFlashesStr = "MuzzleFlashes";
    private const string MuzzlePositionStr = "MuzzlePosition";

    public WeaponDAO(TextAsset textAsset, string name) : base(textAsset, name) { }

    public string GetPath(string name)
    {
        return GetString(name, PathStr);
    }

    public Vector3 GetPosition(string name)
    {
        return GetVector3(name, PositionStr);
    }

    public Vector3 GetADSPosition(string name)
    {
        return GetVector3(name, ADSStr);
    }

    public string GetBullet(string name)
    {
        return GetString(name, BulletStr);
    }

    public float GetSpeed(string name)
    {
        return GetFloat(name, SpeedStr);
    }

    public float GetCooldown(string name)
    {
        return GetFloat(name, CooldownStr);
    }

    public string GetMuzzleFlashesEffectPath(string name)
    {
        return GetString(name, MuzzleFlashesStr);
    }

    public Vector3 GetMuzzlePosition(string name)
    {
        return GetVector3(name, MuzzlePositionStr);
    }
}
