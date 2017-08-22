using UniRx.Triggers;
using UnityEngine;
using UniECS;
using System;
using UniRx;

public class GamePause
{
}

public class GamePlay
{
}

public class PauseSystem : SystemBehaviour
{
	public override void Awake ()
	{
		base.Awake ();

		var PauseEntities = GroupFactory.Create (new Type[] {
			typeof(PauseComponent)	
		});

		PauseEntities.OnAdd ().Subscribe (entitiy => {
			var pauseComponent = entitiy.GetComponent<PauseComponent> ();

			pauseComponent.OnPointerClickAsObservable ().Subscribe (_ => {
				pauseComponent.IsPause.Value = !pauseComponent.IsPause.Value;
			}).AddTo (this.Disposer).AddTo (pauseComponent.Disposer);

			pauseComponent.IsPause.DistinctUntilChanged ().Subscribe (b => {
				if (b) {
					Time.timeScale = 0;
					EventSystem.Publish (new GamePause ());
				} else {
					Time.timeScale = 1;
					EventSystem.Publish (new GamePlay ());
				}
			}).AddTo (this.Disposer).AddTo (pauseComponent.Disposer);

			EventSystem.OnEvent<LoadSceneStart> ().Subscribe (_ => {
				pauseComponent.IsPause.Value = false;
			}).AddTo (this.Disposer).AddTo (pauseComponent.Disposer);
		}).AddTo (this.Disposer);
	}
}
