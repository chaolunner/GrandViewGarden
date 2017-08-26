using UnityEngine;
using UniECS;

public class FollowCamera : ComponentBehaviour
{
	public Camera Camera;
	public Transform Target;
	public Transform Translate;
	public Transform Rotate;
	public Transform Zoom;
	public AnimationCurve Speed = new AnimationCurve (new Keyframe[] {
		new Keyframe (0, 1),
		new Keyframe (1, 1),
	});
}
