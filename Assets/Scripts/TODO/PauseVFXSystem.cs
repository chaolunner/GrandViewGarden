using UnityEngine;
using DG.Tweening;
using System;
using UniECS;
using UniRx;

public class PauseVFXSystem : SystemBehaviour
{
	[Range (0, 1)]
	public float Duration = 0.3f;

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
			var followCamera = followCameraEntity.GetComponent<FollowCamera> ();
			var pausePanel = pausePanelEntity.GetComponent<PausePanel> ();
			var canvasGroup = pausePanelEntity.GetComponent<CanvasGroup> ();
			var gamePanel = gamePanelEntity.GetComponent<GamePanel> ();

			var originFieldOfView = camera.fieldOfView;
			var originAlpha = canvasGroup.alpha;
			var originPosition = gamePanel.transform.position;

			Sequence sequence = null;

			EventSystem.OnEvent<GamePause> ().Subscribe (_ => {
				sequence.Kill ();
				sequence = DOTween.Sequence ()
					.Join (camera.DOFieldOfView (-40, Duration).SetRelative ())
					.Join (canvasGroup.DOFade (1, Duration).SetRelative ())
					.Join (gamePanel.transform.DOMoveY (originPosition.y + 500, Duration))
					.SetUpdate (true);
			}).AddTo (this.Disposer).AddTo (gamePanel.Disposer).AddTo (pausePanel.Disposer).AddTo (followCamera.Disposer);

			EventSystem.OnEvent<GamePlay> ().Subscribe (_ => {
				sequence.Kill ();
				sequence = DOTween.Sequence ()
					.Join (camera.DOFieldOfView (originFieldOfView, Duration))
					.Join (canvasGroup.DOFade (originAlpha, Duration))
					.Join (gamePanel.transform.DOMove (originPosition, Duration))
					.SetUpdate (true);
			}).AddTo (this.Disposer).AddTo (gamePanel.Disposer).AddTo (pausePanel.Disposer).AddTo (followCamera.Disposer);

			return true;
		}).Subscribe ().AddTo (this.Disposer);
	}
}
