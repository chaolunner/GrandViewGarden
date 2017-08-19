using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class RendererExtensions
{
	static public bool IsVisible (this Renderer renderer)
	{
		#if UNITY_EDITOR
		var sceneCamera = SceneView.GetAllSceneCameras ().Where (c => c.name == "SceneCamera").FirstOrDefault ();
		foreach (var camera in Camera.allCameras) {
			var planes = GeometryUtility.CalculateFrustumPlanes (camera);
			if (GeometryUtility.TestPlanesAABB (planes, renderer.bounds) && camera != sceneCamera) {
				return true;
			}
		}
		return false;
		#else
		return renderer.isVisible;
		#endif
	}
}
