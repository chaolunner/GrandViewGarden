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
	private Tweener rotationTweener;
	private Tweener directionTweener;

	void Awake ()
	{
		animator = GetComponent<Animator> ();
	}

	void OnEnable ()
	{
		Go ();
	}

	void OnDisable ()
	{
		Stop ();
	}

	void Start ()
	{
		
	}

	void Go ()
	{
		var index = Random.Range (0, options.Length);
		var selected = options [index];
		var rotation = animator.GetFloat ("Rotation");
		var direction = animator.GetFloat ("Direction");
		var duration = Mathf.Abs (selected - rotation) / (Speed * Time.unscaledDeltaTime);

		rotationTweener = DOTween.To (() => rotation, setter => animator.SetFloat ("Rotation", setter), selected, duration);
		rotationTweener.SetEase (Ease.InOutQuad);
		rotationTweener.SetDelay (Delay);
		rotationTweener.OnComplete (() => {
			Stop ();
			Go ();
		});

		if (selected != rotation) {
			directionTweener = DOTween.To (() => direction, setter => animator.SetFloat ("Direction", setter), selected < rotation ? 1 : -1, 0.5f * duration);
			directionTweener.SetEase (Ease.InQuad);
			directionTweener.SetDelay (Delay);
			directionTweener.OnComplete (() => {
				direction = animator.GetFloat ("Direction");
				directionTweener = DOTween.To (() => direction, setter => animator.SetFloat ("Direction", setter), 0, 0.5f * duration);
				directionTweener.SetEase (Ease.OutQuad);
			});
		}
	}

	void Stop ()
	{
		if (rotationTweener != null) {
			rotationTweener.Kill ();
		}
		if (directionTweener != null) {
			directionTweener.Kill ();
		}
	}
}
