using UniRx.Triggers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniECS;
using System;
using UniRx;

public class SettlementSystem : SystemBehaviour
{
	public override void Awake ()
	{
		base.Awake ();

		var SettlementEntities = GroupFactory.Create (new Type[] {
			typeof(Settlement)
		});
		SettlementEntities.OnAdd ().Subscribe (entity => {
			var settlement = entity.GetComponent<Settlement> ();
			Vector3 position = settlement.ScreenPosition.position;
			settlement.OnTriggerEnterAsObservable ().Subscribe (data => {
				if (data.tag == "Settlement") {
					settlement.SettlementPanel.position = position;
				}	
			}).AddTo (this.Disposer).AddTo (settlement.Disposer);
		}).AddTo (this.Disposer);
	}
}
