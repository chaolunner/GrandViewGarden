using UnityEditor;
using UniRx;

[CustomPropertyDrawer(typeof(GameObjectReactiveProperty))]
[CustomPropertyDrawer(typeof(TransformReactiveProperty))]
[CustomPropertyDrawer(typeof(RendererReactiveProperty))]
[CustomPropertyDrawer(typeof(FadeStateReactiveProperty))]
public partial class ExtendInspectorDisplayDrawer : InspectorDisplayDrawer
{
}
