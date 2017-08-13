using UnityEngine;
using DG.Tweening;

public class FreeDriving : MonoBehaviour
{
	[Range (1, 30)]
	public float Speed = 10;
	[Range (0, 10)]
	public float Delay = 1;
	private readonly float[] options = new float [] { -1, -0.25f, 0.5f };
	private Animator animator;
	private Tweener tweener;

	void Start ()
	{
		animator = GetComponent<Animator> ();

		Go ();
	}

	void Go ()
	{
		var index = Random.Range (0, options.Length);
		var selected = options [index];
		var rotation = animator.GetFloat ("Rotation");

		tweener = DOTween.To (() => rotation, setter => animator.SetFloat ("Rotation", setter), selected, Mathf.Abs (selected - rotation) / (Speed * Time.unscaledDeltaTime));
		tweener.SetDelay (Delay);
		tweener.SetEase (Ease.InOutQuad);
		tweener.OnStart (() => {
			if (selected != rotation) {
				if (selected < rotation) {
					animator.SetFloat ("Direction", 1);
				} else {
					animator.SetFloat ("Direction", -1);
				}
			}
		});
		tweener.OnComplete (() => {
			Go ();
		});
	}

	void Stop ()
	{
		if (tweener != null) {
			tweener.Kill ();
		}
	}
}
