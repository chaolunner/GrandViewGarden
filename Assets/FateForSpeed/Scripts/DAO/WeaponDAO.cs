using UnityEngine;
using UniEasy;

public class WeaponDAO
{
    private EasyCsv reader;
    private const string NameStr = "Name";
    private const string BulletStr = "Bullet";

    public WeaponDAO(TextAsset textAsset)
    {
        reader = new EasyCsv(textAsset);
    }

    public string GetBullet(string name)
    {
        return reader.GetValue(NameStr, name, BulletStr);
    }
}
