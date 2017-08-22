using UnityEngine;
using UniECS;

public class FollowCamera : ComponentBehaviour
{
	public Transform Target;
	public AnimationCurve Speed = new AnimationCurve (new Keyframe[] {
		new Keyframe (0, 1),
		new Keyframe (1, 1),
	});
}
