using UnityEngine.SceneManagement;
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

		LoadSceneEntities.OnAdd ().Subscribe (entity => {
			var loadScene = entity.GetComponent<LoadScene> ();

			EventSystem.OnEvent<ClickCenterEvent> ()
				.Where (_ => !string.IsNullOrEmpty (loadScene.SceneName))
				.Subscribe (e => {
				if (loadScene.transform == e.Target) {
					SceneManager.LoadScene (loadScene.SceneName);
				}
			}).AddTo (this.Disposer).AddTo (loadScene.Disposer);
		}).AddTo (this.Disposer);
	}
}
