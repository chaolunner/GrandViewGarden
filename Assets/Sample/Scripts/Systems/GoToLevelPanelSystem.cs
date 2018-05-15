using UnityEngine.SceneManagement;
using UnityEngine;
using UniECS;
using System;
using UniRx;

public class GoToLevelPanelSystem : SystemBehaviour
{
	public override void Awake ()
	{
		base.Awake ();

		var GoToLevelPanelEntities = GroupFactory.Create (new Type[] {
			typeof(ExchangePosition),
			typeof(GoToLevelPanel),
		});

		GoToLevelPanelEntities.OnAdd ().Subscribe (entity => {
			var exchangePosition = entity.GetComponent<ExchangePosition> ();

			if (LoadSceneHistory.PreviousSceneName == "Gameplay") {
				exchangePosition.IsOn.Value = true;
			}
		}).AddTo (this.Disposer);
	}
}
