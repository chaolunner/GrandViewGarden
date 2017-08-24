using UnityEngine;
using DG.Tweening;
using System;
using UniECS;
using UniRx;

public class PauseVFXSystem : SystemBehaviour
{
	public override void Awake ()
	{
		base.Awake ();

		var FollowCameraEntities = GroupFactory.Create (new Type[] {
			typeof(FollowCamera),
			typeof(Camera),
		});

		var PausePanelEntities = GroupFactory.Create (new Type[] {
			typeof(PausePanel)
		});

		var GamePanelEntities = GroupFactory.Create (new Type[] {
			typeof(GamePanel)
		});

		Observable.CombineLatest (FollowCameraEntities.OnAdd (), PausePanelEntities.OnAdd (), GamePanelEntities.OnAdd (), (followCameraEntity, pausePanelEntity, gamePanelEntity) => {
			var camera = followCameraEntity.GetComponent<Camera> ();
			var canvasGroup = pausePanelEntity.GetComponent<CanvasGroup> ();
			var gamePanel = gamePanelEntity.GetComponent<GamePanel> ();

			var originFieldOfView = camera.fieldOfView;
			var originAlpha = canvasGroup.alpha;
			var originPosition = gamePanel.transform.position;

			Sequence sequence = null;

			EventSystem.OnEvent<GamePause> ().Subscribe (_ => {
				sequence.Kill ();
				sequence = DOTween.Sequence ()
					.Join (camera.DOFieldOfView (30, 1).SetRelative ())
					.Join (canvasGroup.DOFade (1, 1).SetRelative ())
					.Join (gamePanel.transform.DOMoveY (originPosition.y + 500, 1))
					.SetUpdate (true);
			});

			EventSystem.OnEvent<GamePlay> ().Subscribe (_ => {
				sequence.Kill ();
				sequence = DOTween.Sequence ()
					.Join (camera.DOFieldOfView (originFieldOfView, 1))
					.Join (canvasGroup.DOFade (originAlpha, 1))
					.Join (gamePanel.transform.DOMove (originPosition, 1))
					.SetUpdate (true);
			});

			return true;
		}).Subscribe ().AddTo (this.Disposer);
	}
}
