using UnityEngine.EventSystems;
using UniRx.Triggers;
using UnityEngine;
using System.Linq;
using DG.Tweening;
using UniRx;

public class ExchangePosition : MonoBehaviour
{
	public BoolReactiveProperty IsOn;
	public Transform[] Origins;
	public Transform[] Targets;
	[Range (0, 1)]
	public float Duration = 0.5f;
	[Range (0, 2)]
	public float Delay = 0;

	void Start ()
	{
		IsOn.DistinctUntilChanged ().Where (b => b == true).Subscribe (_ => {
			DoExchange ();
		});

		gameObject.OnPointerClickAsObservable ().Subscribe (_ => {
			IsOn.Value = true;
		});

		foreach (var target in Targets) {
			target.gameObject.SetActive (false);
		}
	}

	void DoExchange ()
	{
		if (Origins == null || Targets == null || Origins.Length <= 0 || Origins.Length != Targets.Length) {
			return;
		}
		for (int i = 0; i < Origins.Length; i++) {
			var index = i;
			var originPosition = Origins [index].position;
			var tweener = Origins [index].DOMove (Targets [index].position, Duration);
			tweener.SetDelay (Delay);
			tweener.SetEase (Ease.InBack);
			tweener.OnComplete (() => {
				Targets [index].gameObject.SetActive (!Targets [index].gameObject.activeSelf);
				Origins [index].gameObject.SetActive (!Origins [index].gameObject.activeSelf);
				Targets [index].DOMove (originPosition, Duration).SetEase (Ease.OutBack);
				IsOn.Value = false;
			});
		}
	}
}
