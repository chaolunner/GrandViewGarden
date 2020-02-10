using System.Collections.Generic;
using UniEasy.Editor;
using UnityEditor;
using UnityEngine;
using UniEasy;

public static class MultiBlockSetupDrawer
{
    [MenuItem("Assets/Multi Block Setup/Save Selected Blocks As A BlockSetup")]
    private static void SaveSelectedBlocksAsABlockSetup()
    {
        var blockSetup = ScriptableObject.CreateInstance<MultiBlockSetup>();

        blockSetup.Blocks = new List<EasyBlock>();
        blockSetup.Blocks.AddRange(Selection.GetFiltered<EasyBlock>(SelectionMode.Assets));

        ScriptableObjectUtility.CreateAssetWithSavePrompt(blockSetup, "BlockSetup");
    }

    [MenuItem("Assets/Multi Block Setup/Save Selected Blocks As A BlockSetup", true)]
    private static bool HasBlockFileSelected()
    {
        var blocks = Selection.GetFiltered<EasyBlock>(SelectionMode.Assets);

        return blocks != null && blocks.Length > 0;
    }
}
