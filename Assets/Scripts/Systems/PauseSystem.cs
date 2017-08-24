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
	public BoolReactiveProperty IsPause = new BoolReactiveProperty ();

	public override void Awake ()
	{
		base.Awake ();

		var PauseEntities = GroupFactory.Create (new Type[] {
			typeof(InteractiveComponent),
			typeof(PauseComponent),
		});

		PauseEntities.OnAdd ().Subscribe (entitiy => {
			var pauseComponent = entitiy.GetComponent<PauseComponent> ();
			var interactiveComponent = entitiy.GetComponent<InteractiveComponent> ();

			foreach (var touchArea in interactiveComponent.TouchAreas) {
				touchArea.OnPointerClickAsObservable ().Subscribe (_ => {
					IsPause.Value = !IsPause.Value;
				}).AddTo (this.Disposer).AddTo (pauseComponent.Disposer);
			}

			IsPause.DistinctUntilChanged ().Subscribe (b => {
				if (b) {
					Time.timeScale = 0;
					EventSystem.Publish (new GamePause ());
				} else {
					Time.timeScale = 1;
					EventSystem.Publish (new GamePlay ());
				}
			}).AddTo (this.Disposer).AddTo (pauseComponent.Disposer);

			EventSystem.OnEvent<LoadSceneStart> ().Subscribe (_ => {
				IsPause.Value = false;
			}).AddTo (this.Disposer).AddTo (pauseComponent.Disposer);
		}).AddTo (this.Disposer);
	}
}
