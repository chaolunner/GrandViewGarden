using UniRx.Triggers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniECS;
using System;
using UniRx;

public class PauseContinueSytem : SystemBehaviour
{
	public override void Awake ()
	{
		base.Awake ();

		var PauseContinueEntities = GroupFactory.Create (new Type[] {
			typeof(PauseContinue)	
		});

		PauseContinueEntities.OnAdd ().Subscribe (entitiy => {

			var pauseContinue = entitiy.GetComponent<PauseContinue> ();
			Vector3 pausepos = pauseContinue.pausePanel.position;
			Vector3 gamePos = pauseContinue.gamePanel.position;
			bool pause = false;

			pauseContinue.pauseBtn.OnClickAsObservable ().Subscribe (_ => {
				pauseContinue.gamePanel.position = pausepos;
				pauseContinue.pausePanel.position = gamePos;
				pause = true;
			});
			pauseContinue.contBtn.OnClickAsObservable ().Subscribe (_ => {
				pauseContinue.gamePanel.position = gamePos;
				pauseContinue.pausePanel.position = pausepos;
				pause = false;
			});

			Observable.EveryUpdate ().Subscribe (_ => {
				if (pause) {
					Time.timeScale = 0;			
				} else {
					Time.timeScale = 1;	
				}
			}).AddTo (this.Disposer).AddTo (pauseContinue.Disposer);
		}).AddTo (this.Disposer);
	}
}
