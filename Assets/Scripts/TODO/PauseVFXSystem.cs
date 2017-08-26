using UnityEngine;
using DG.Tweening;
using System;
using UniECS;
using UniRx;

public class PauseVFXSystem : SystemBehaviour
{
	[Range (0, 2)]
	public float Duration = 0.5f;
	[Range (0, 30)]
	public float Zoom = 25;

	public override void Awake ()
	{
		base.Awake ();

		var FollowCameraEntities = GroupFactory.Create (new Type[] {
			typeof(FollowCamera)
		});

		var PausePanelEntities = GroupFactory.Create (new Type[] {
			typeof(PausePanel)
		});

		var GamePanelEntities = GroupFactory.Create (new Type[] {
			typeof(GamePanel)
		});

		Observable.CombineLatest (FollowCameraEntities.OnAdd (), PausePanelEntities.OnAdd (), GamePanelEntities.OnAdd (), (followCameraEntity, pausePanelEntity, gamePanelEntity) => {
			var followCamera = followCameraEntity.GetComponent<FollowCamera> ();
			var pausePanel = pausePanelEntity.GetComponent<PausePanel> ();
			var canvasGroup = pausePanelEntity.GetComponent<CanvasGroup> ();
			var gamePanel = gamePanelEntity.GetComponent<GamePanel> ();

			var direction = Vector3.Normalize (Vector3.ProjectOnPlane (followCamera.Camera.transform.forward, Vector3.up));
			var originCameraPosition = followCamera.Translate.transform.position;
			var originGamePanelPosition = gamePanel.transform.position;

			Sequence sequence = null;

			EventSystem.OnEvent<GamePause> ().Subscribe (_ => {
				originCameraPosition = followCamera.transform.position;
				sequence.Kill ();
				sequence = DOTween.Sequence ()
					.Join (followCamera.Translate.transform.DOMove (originCameraPosition + Zoom * direction, Duration))
					.Join (canvasGroup.DOFade (1, Duration))
					.Join (gamePanel.transform.DOMoveY (originGamePanelPosition.y + 500, Duration))
					.SetUpdate (true);
			}).AddTo (this.Disposer).AddTo (gamePanel.Disposer).AddTo (pausePanel.Disposer).AddTo (followCamera.Disposer);

			EventSystem.OnEvent<GamePlay> ().Subscribe (_ => {
				sequence.Kill ();
				sequence = DOTween.Sequence ()
					.Join (followCamera.Translate.transform.DOMove (originCameraPosition, Duration))
					.Join (canvasGroup.DOFade (0, Duration))
					.Join (gamePanel.transform.DOMove (originGamePanelPosition, Duration))
					.SetUpdate (true);
			}).AddTo (this.Disposer).AddTo (gamePanel.Disposer).AddTo (pausePanel.Disposer).AddTo (followCamera.Disposer);

			return true;
		}).Subscribe ().AddTo (this.Disposer);
	}
}
