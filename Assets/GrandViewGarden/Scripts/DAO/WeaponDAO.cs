using UnityEngine;
using UniEasy;

public class WeaponDAO
{
    private EasyCsv reader;
    private const char Separator = ',';
    private const string NameStr = "Name";
    private const string PathStr = "Path";
    private const string PositionStr = "Position";
    private const string ADSStr = "ADS";
    private const string BulletStr = "Bullet";
    private const string SpeedStr = "Speed";
    private const string CooldownStr = "Cooldown";

    public WeaponDAO(TextAsset textAsset)
    {
        reader = new EasyCsv(textAsset);
    }

    public string GetPath(string name)
    {
        return reader.GetValue(NameStr, name, PathStr);
    }

    public Vector3 GetLocalPosition(string name)
    {
        var str = reader.GetValue(NameStr, name, PositionStr).Split(Separator);
        if (str.Length == 1)
        {
            return new Vector3(float.Parse(str[0]), float.Parse(str[0]), float.Parse(str[0]));
        }
        else if (str.Length >= 3)
        {
            return new Vector3(float.Parse(str[0]), float.Parse(str[1]), float.Parse(str[2]));
        }
        return Vector3.zero;
    }

    public Vector3 GetADSPosition(string name)
    {
        var str = reader.GetValue(NameStr, name, ADSStr).Split(Separator);
        if (str.Length == 1)
        {
            return new Vector3(float.Parse(str[0]), float.Parse(str[0]), float.Parse(str[0]));
        }
        else if (str.Length >= 3)
        {
            return new Vector3(float.Parse(str[0]), float.Parse(str[1]), float.Parse(str[2]));
        }
        return Vector3.zero;
    }

    public string GetBullet(string name)
    {
        return reader.GetValue(NameStr, name, BulletStr);
    }

    public float GetSpeed(string name)
    {
        return float.Parse(reader.GetValue(NameStr, name, SpeedStr));
    }

    public float GetCooldown(string name)
    {
        return float.Parse(reader.GetValue(NameStr, name, CooldownStr));
    }
}
