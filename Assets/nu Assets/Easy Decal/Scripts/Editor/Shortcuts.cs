using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace ch.sycoforge.Decal.Editor
{
    public class Shortcuts
    {
        /// </summary>
        [MenuItem("Edit/Easy Decal/Duplicate as Prefab     Shift + D")]
        static void InstantiatePrefab()
        {
            EasyDecalEditor.DuplicateDecalPrefab();
        }

        /// <summary>
        /// Alt + D
        /// </summary>
        [MenuItem("Edit/Easy Decal/Duplicate [Deep] &d")]
        static void InstantiateClone()
        {
            EasyDecalEditor.DuplicateDecal();
        }

        /// <summary>
        /// Alt + C
        /// </summary>
        [MenuItem("Edit/Easy Decal/Convert to Mesh &c")]
        static void InstantiateMeshClone()
        {
            EasyDecalEditor.ToDecalMesh();
        }
    }
}
