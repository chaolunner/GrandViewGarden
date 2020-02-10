using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;
using UniEasy;
using System;

[Serializable]
public class SceneSetup
{
    public bool IsActive;
    public bool IsLoaded;
    public LoadSceneMode Mode;
    public string Path;

#if UNITY_EDITOR
    public SceneSetup(UnityEditor.SceneManagement.SceneSetup setup, LoadSceneMode mode = LoadSceneMode.Additive)
    {
        IsActive = setup.isActive;
        IsLoaded = setup.isLoaded;
        Mode = mode;
        Path = setup.path;
    }
#endif
}

public class MultiSceneSetup : ScriptableObject
{
    [EnumMask]
    public LoadingScreenLayer LoadingMask = (LoadingScreenLayer)(1 << 0);
    [Reorderable(elementName: "Scene")]
    public List<SceneSetup> Setups;
}
