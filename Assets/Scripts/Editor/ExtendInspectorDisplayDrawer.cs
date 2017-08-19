using UnityEditor;
using UnityEngine;
using UniRx;

[CustomPropertyDrawer (typeof(GameObjectReactiveProperty))]
[CustomPropertyDrawer (typeof(TransformReactiveProperty))]
[CustomPropertyDrawer (typeof(RendererReactiveProperty))]
public class ExtendInspectorDisplayDrawer : InspectorDisplayDrawer
{
}
