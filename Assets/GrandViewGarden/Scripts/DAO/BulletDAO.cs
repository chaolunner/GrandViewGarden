using UnityEngine;

public class BulletDAO : CsvDAO
{
    private const string PathStr = "Path";
    private const string HoleSizeStr = "HoleSize";

    public BulletDAO(TextAsset textAsset, string name) : base(textAsset, name) { }

    public string GetPath(string name)
    {
        return GetString(name, PathStr);
    }

    public float GetHoleSize(string name)
    {
        return GetFloat(name, HoleSizeStr);
    }
}
