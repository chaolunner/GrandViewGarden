using UnityEngine.SceneManagement;
using UniRx.Triggers;
using UnityEngine;
using UniECS;
using System;
using UniRx;

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
			typeof(FadeInTag),
			typeof(FadeIn),
		});

		FadeInEntities.OnAdd ().Subscribe (fadeInEntity => {
			var fadeIn = fadeInEntity.GetComponent<FadeIn> ();
			var fadeInTag = fadeInEntity.GetComponent<FadeInTag> ();

			if (fadeInTag.FadeInType == FadeInType.Scene) {
				LoadSceneEntities.OnAdd ().Subscribe (loadSceneEntity => {
					var loadScene = loadSceneEntity.GetComponent<LoadScene> ();

					loadScene.OnPointerClickAsObservable ().Where (_ => loadScene.BindToClick).Select (_ => true)
						.Merge (EventSystem.OnEvent<ClickCenterEvent> ().Where (e => !string.IsNullOrEmpty (loadScene.SceneName) && loadScene.transform == e.Target).Select (_ => true))
						.Subscribe (unused => {
						fadeIn.Alpha.Value = 1;
						Observable.Timer (TimeSpan.FromSeconds (fadeIn.Duration)).Subscribe (_ => {
							SceneManager.LoadSceneAsync (loadScene.SceneName).ToObservable ().DoOnCompleted (() => {
								fadeIn.Alpha.Value = 0;
							}).Subscribe ().AddTo (this.Disposer).AddTo (loadScene.Disposer).AddTo (fadeIn.Disposer);
						}).AddTo (this.Disposer).AddTo (loadScene.Disposer).AddTo (fadeIn.Disposer);
					}).AddTo (this.Disposer).AddTo (loadScene.Disposer).AddTo (fadeIn.Disposer);
				}).AddTo (this.Disposer).AddTo (fadeIn.Disposer);
			}
		}).AddTo (this.Disposer);
	}
}
