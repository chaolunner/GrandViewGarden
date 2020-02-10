using UnityEditor.Build.Reporting;
using System.Collections.Generic;
using UnityEditor;
using System;

public class BuildUtility
{
    private static string[] Scenes = FindEnabledEditorScenes();

    private static string FileNameWithoutExtension = "GrandViewGarden";
    private static string Path = "Builds";

    [MenuItem("Tools/Builds/Build Android")]
    public static void PerformAndroidBuild()
    {
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        string name = FileNameWithoutExtension + ".apk";
        GenericBuild(Scenes, Path + "/" + name, BuildTargetGroup.Android, BuildTarget.Android, BuildOptions.None);
    }

    private static string[] FindEnabledEditorScenes()
    {
        List<string> EditorScenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled) continue;
            EditorScenes.Add(scene.path);
        }
        return EditorScenes.ToArray();
    }

    private static void GenericBuild(string[] scenes, string path, BuildTargetGroup group, BuildTarget target, BuildOptions options)
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(group, target);
        var report = BuildPipeline.BuildPlayer(scenes, path, target, options);
        if (report.summary.result == BuildResult.Failed)
        {
            throw new Exception("BuildPlayer failure: " + report.summary);
        }
    }
}
