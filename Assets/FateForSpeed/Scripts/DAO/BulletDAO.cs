using UnityEngine;
using UniEasy;

public class BulletDAO
{
    private EasyCsv reader;
    private const string NameStr = "Name";
    private const string PathStr = "Path";

    public BulletDAO(TextAsset textAsset)
    {
        reader = new EasyCsv(textAsset);
    }

    public string GetPath(string name)
    {
        return reader.GetValue(NameStr, name, PathStr);
    }
}
