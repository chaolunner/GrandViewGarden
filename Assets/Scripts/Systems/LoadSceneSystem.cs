using UnityEngine.SceneManagement;
using UniRx.Triggers;
using UnityEngine;
using UniECS;
using System;
using UniRx;

public class LoadSceneStart
{
}

public class LoadSceneCompleted
{
}

public class LoadSceneSystem : SystemBehaviour
{
	public override void Awake ()
	{
		base.Awake ();

		var LoadSceneEntities = GroupFactory.Create (new Type[] {
			typeof(LoadScene)
		});

		var FadeInEntities = GroupFactory.Create (new Type[] {
			typeof(CanvasGroup),
			typeof(FadeIn),
		});

		FadeInEntities.OnAdd ().Subscribe (fadeInEntity => {
			var fadeIn = fadeInEntity.GetComponent<FadeIn> ();

			if (fadeIn.FadeInType == FadeInType.Scene) {
				LoadSceneEntities.OnAdd ().Subscribe (loadSceneEntity => {
					var loadScene = loadSceneEntity.GetComponent<LoadScene> ();

					loadScene.OnPointerClickAsObservable ().Where (_ => loadScene.BindToClick).Select (_ => true)
						.Merge (EventSystem.OnEvent<ClickCenterEvent> ().Where (e => loadScene.transform == e.Target).Select (_ => true))
						.Where (_ => !string.IsNullOrEmpty (loadScene.SceneName))
						.Subscribe (unused => {
						fadeIn.NormalizedTime.Value = 1;
						EventSystem.Publish (new LoadSceneStart ());
						Observable.Timer (TimeSpan.FromSeconds (fadeIn.Duration)).Subscribe (_unused => {
							SceneManager.LoadSceneAsync (loadScene.SceneName).ToObservable ().DoOnCompleted (() => {
								fadeIn.NormalizedTime.Value = 0;
								Observable.Timer (TimeSpan.FromSeconds (fadeIn.Duration)).Subscribe (_ => {
									EventSystem.Publish (new LoadSceneCompleted ());
								}).AddTo (this.Disposer).AddTo (loadScene.Disposer).AddTo (fadeIn.Disposer);
							}).Subscribe ().AddTo (this.Disposer).AddTo (loadScene.Disposer).AddTo (fadeIn.Disposer);
						}).AddTo (this.Disposer).AddTo (loadScene.Disposer).AddTo (fadeIn.Disposer);
					}).AddTo (this.Disposer).AddTo (loadScene.Disposer).AddTo (fadeIn.Disposer);
				}).AddTo (this.Disposer).AddTo (fadeIn.Disposer);
			}
		}).AddTo (this.Disposer);
	}
}
