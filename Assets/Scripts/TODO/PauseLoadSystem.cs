using UniRx;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using UniECS;
using UniRx;

public class PauseLoadSystem : SystemBehaviour
{
	public override void Awake ()
	{
		base.Awake ();
		var FollowCameraEntities = GroupFactory.Create (new Type[] {
			typeof(FollowCamera),
			typeof(Camera),
		});
		var PausePanelForPauseEntities = GroupFactory.Create (new Type[] {
			typeof(PausePanelForPause)	
		});
		var GamePanelForPauseEntities = GroupFactory.Create (new Type[] {
			typeof(GamePanelForPause)	
		});

		FollowCameraEntities.OnAdd ().Subscribe (entity => {
			var followCamera = entity.GetComponent<FollowCamera> ();
			var camera = entity.GetComponent<Camera> ();
			EventSystem.OnEvent<GamePause> ().Subscribe (_ => {
				camera.fieldOfView -= 10;
				if (camera.fieldOfView <= 20) {
					camera.fieldOfView = 20;
				}
			});
			EventSystem.OnEvent<GamePlay> ().Subscribe (_ => {
				camera.fieldOfView += 10;
				if (camera.fieldOfView >= 60f) {
					camera.fieldOfView = 60;
				}
			});
		}).AddTo (this.Disposer);

		PausePanelForPauseEntities.OnAdd ().Subscribe (entity => {
			var pausePanelForPause = entity.GetComponent<PausePanelForPause> ();
			EventSystem.OnEvent<GamePause> ().Subscribe (_ => {
				pausePanelForPause.pausePanel.SetActive (true);
				pausePanelForPause.canvasGroup.alpha = 1;
			});
			EventSystem.OnEvent<GamePlay> ().Subscribe (_ => {				
				pausePanelForPause.canvasGroup.alpha = 0;
				pausePanelForPause.pausePanel.SetActive (false);
			});
		}).AddTo (this.Disposer);

		GamePanelForPauseEntities.OnAdd ().Subscribe (entity => {
			var gamePanelForPause = entity.GetComponent<GamePanelForPause> ();
			EventSystem.OnEvent<GamePause> ().Subscribe (_ => {
				transform.DOScale (Vector2.one * gamePanelForPause.scale, gamePanelForPause.duration);
			});
			EventSystem.OnEvent<GamePlay> ().Subscribe (_ => {
				transform.DOScale (Vector2.one, gamePanelForPause.duration);
			});
		}).AddTo (this.Disposer);
	}
}
