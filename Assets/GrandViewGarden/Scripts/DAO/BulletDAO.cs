using UnityEngine;

public class BulletDAO : CsvDAO
{
    private const string PathStr = "Path";

    public BulletDAO(TextAsset textAsset, string name) : base(textAsset, name) { }

    public string GetPath(string name)
    {
        return GetString(name, PathStr);
    }
}
