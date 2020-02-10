using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UniEasy.Editor;
using UnityEditor;
using UnityEngine;

public static class MultiSceneSetupDrawer
{
    [MenuItem("Assets/Multi Scene Setup/Create")]
    public static void CreateNewSceneSetup()
    {
        SaveCurrentSceneSetup();
    }

    [MenuItem("Assets/Multi Scene Setup/Create", true)]
    public static bool CreateNewSceneSetupValidate()
    {
        return PathsUtility.TryGetSelectedFolderPathInProjectsTab() != null;
    }

    [MenuItem("Assets/Multi Scene Setup/Overwrite")]
    public static void SaveSceneSetup()
    {
        var assetPath = PathsUtility.ConvertFullAbsolutePathToAssetPath(PathsUtility.TryGetSelectedFilePathInProjectsTab());

        SaveCurrentSceneSetup(assetPath);
    }

    [MenuItem("Assets/Multi Scene Setup/Load")]
    public static void RestoreSceneSetup()
    {
        var assetPath = PathsUtility.ConvertFullAbsolutePathToAssetPath(PathsUtility.TryGetSelectedFilePathInProjectsTab());
        var loader = AssetDatabase.LoadAssetAtPath<MultiSceneSetup>(assetPath);
        var setups = new UnityEditor.SceneManagement.SceneSetup[loader.Setups.Count];

        for (var i = 0; i < loader.Setups.Count; i++)
        {
            setups[i] = new UnityEditor.SceneManagement.SceneSetup();
            setups[i].isActive = loader.Setups[i].IsActive;
            setups[i].isLoaded = loader.Setups[i].IsLoaded;
            setups[i].path = loader.Setups[i].Path;
        }

        EditorSceneManager.RestoreSceneManagerSetup(setups);
    }

    [MenuItem("Assets/Multi Scene Setup", true)]
    public static bool SceneSetupRootValidate()
    {
        return HasSceneSetupFileSelected();
    }

    [MenuItem("Assets/Multi Scene Setup/Overwrite", true)]
    public static bool SaveSceneSetupValidate()
    {
        return HasSceneSetupFileSelected();
    }

    [MenuItem("Assets/Multi Scene Setup/Load", true)]
    public static bool RestoreSceneSetupValidate()
    {
        return HasSceneSetupFileSelected();
    }

    private static bool HasSceneSetupFileSelected()
    {
        return PathsUtility.TryGetSelectedFilePathInProjectsTab() != null;
    }

    private static void SaveCurrentSceneSetup(string assetPath = null)
    {
        var loader = ScriptableObject.CreateInstance<MultiSceneSetup>();
        var setups = EditorSceneManager.GetSceneManagerSetup();

        loader.Setups = new List<SceneSetup>();
        for (var i = 0; i < setups.Length; i++)
        {
            loader.Setups.Add(new SceneSetup(setups[i], i == 0 ? LoadSceneMode.Single : LoadSceneMode.Additive));
        }

        if (string.IsNullOrEmpty(assetPath))
        {
            ScriptableObjectUtility.CreateAssetWithSavePrompt(loader, "SceneSetup");
        }
        else
        {
            ScriptableObjectUtility.CreateAsset(loader, assetPath);
        }
    }
}
