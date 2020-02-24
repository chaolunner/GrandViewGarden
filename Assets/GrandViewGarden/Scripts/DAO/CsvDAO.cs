using UnityEngine;
using UniEasy;

public class CsvDAO
{
    private EasyCsv reader;
    private string header;
    private const char Semicolon = ';';

    public CsvDAO(TextAsset textAsset)
    {
        reader = new EasyCsv(textAsset);
    }

    public CsvDAO(TextAsset textAsset, string name)
    {
        reader = new EasyCsv(textAsset);
        header = name;
    }

    public string GetString(string rowName, string columnName)
    {
        return GetString(header, rowName, columnName);
    }

    public string GetString(string name, string rowName, string columnName)
    {
        return reader.GetValue(name, rowName, columnName);
    }

    public int GetInt(string rowName, string columnName)
    {
        return GetInt(header, rowName, columnName);
    }

    public int GetInt(string name, string rowName, string columnName)
    {
        int result;
        int.TryParse(GetString(name, rowName, columnName), out result);
        return result;
    }

    public float GetFloat(string rowName, string columnName)
    {
        return GetFloat(header, rowName, columnName);
    }

    public float GetFloat(string name, string rowName, string columnName)
    {
        float result;
        float.TryParse(GetString(name, rowName, columnName), out result);
        return result;
    }

    public Vector2 GetVector2(string rowName, string columnName)
    {
        return GetVector2(header, rowName, columnName);
    }

    public Vector2 GetVector2(string name, string rowName, string columnName)
    {
        var strs = GetString(name, rowName, columnName).Split(Semicolon);
        if (strs.Length == 1)
        {
            return new Vector2(float.Parse(strs[0]), float.Parse(strs[0]));
        }
        else if (strs.Length >= 2)
        {
            return new Vector2(float.Parse(strs[0]), float.Parse(strs[1]));
        }
        return Vector2.zero;
    }

    public Vector3 GetVector3(string rowName, string columnName)
    {
        return GetVector3(header, rowName, columnName);
    }

    public Vector3 GetVector3(string name, string rowName, string columnName)
    {
        var strs = GetString(name, rowName, columnName).Split(Semicolon);
        if (strs.Length == 1)
        {
            return new Vector3(float.Parse(strs[0]), float.Parse(strs[0]), float.Parse(strs[0]));
        }
        else if (strs.Length >= 3)
        {
            return new Vector3(float.Parse(strs[0]), float.Parse(strs[1]), float.Parse(strs[2]));
        }
        return Vector3.zero;
    }
}
