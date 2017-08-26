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
			var gamePanelRectTransform = gamePanel.transform as RectTransform;

			Sequence sequence = null;

			pausePanel.Zoom.localScale = Vector3.zero;
			canvasGroup.blocksRaycasts = false;

			EventSystem.OnEvent<GamePause> ().Subscribe (_ => {
				originCameraPosition = followCamera.transform.position;
				pausePanel.Zoom.localScale = 0.8f * Vector3.one;

				sequence.Kill (true);
				sequence = DOTween.Sequence ()
					.Join (followCamera.Translate.transform.DOMove (originCameraPosition + 25 * direction, 0.5f))
					.Join (canvasGroup.DOFade (1, 1))
					.Join (pausePanel.Zoom.DOScale (Vector3.one, 1).SetEase (Ease.OutQuad))
					.Join (DOTween.To (() => gamePanelRectTransform.anchoredPosition.y, setter => gamePanelRectTransform.anchoredPosition = new Vector2 (gamePanelRectTransform.anchoredPosition.x, setter), 200, 1))
					.SetUpdate (true);
				sequence.OnComplete (() => {
					canvasGroup.blocksRaycasts = true;
				});
			}).AddTo (this.Disposer).AddTo (gamePanel.Disposer).AddTo (pausePanel.Disposer).AddTo (followCamera.Disposer);

			EventSystem.OnEvent<GamePlay> ().Subscribe (_ => {
				pausePanel.Zoom.localScale = Vector3.zero;
				canvasGroup.blocksRaycasts = false;

				sequence.Kill (true);
				sequence = DOTween.Sequence ()
					.Join (followCamera.Translate.transform.DOMove (originCameraPosition, 1))
					.Join (canvasGroup.DOFade (0, 1))
					.Join (DOTween.To (() => gamePanelRectTransform.anchoredPosition.y, setter => gamePanelRectTransform.anchoredPosition = new Vector2 (gamePanelRectTransform.anchoredPosition.x, setter), 0, 1))
					.SetUpdate (true);
			}).AddTo (this.Disposer).AddTo (gamePanel.Disposer).AddTo (pausePanel.Disposer).AddTo (followCamera.Disposer);

			return true;
		}).Subscribe ().AddTo (this.Disposer);
	}
}
